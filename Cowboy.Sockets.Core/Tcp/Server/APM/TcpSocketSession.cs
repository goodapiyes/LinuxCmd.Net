﻿using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Cowboy.Buffer;
using Serilog;

namespace Cowboy.Sockets.Core
{
    public sealed class TcpSocketSession
    {
        #region Fields

        private static readonly ILogger _log = LogHelper.Logger;
        private TcpClient _tcpClient;
        private readonly TcpSocketServerConfiguration _configuration;
        private readonly ISegmentBufferManager _bufferManager;
        private readonly TcpSocketServer _server;
        private readonly string _sessionKey;
        private Stream _stream;
        private ArraySegment<byte> _receiveBuffer = default(ArraySegment<byte>);
        private int _receiveBufferOffset = 0;
        private IPEndPoint _remoteEndPoint;
        private IPEndPoint _localEndPoint;

        private int _state;
        private const int _none = 0;
        private const int _connecting = 1;
        private const int _connected = 2;
        private const int _disposed = 5;

        #endregion

        #region Constructors

        public TcpSocketSession(
            TcpClient tcpClient,
            TcpSocketServerConfiguration configuration,
            ISegmentBufferManager bufferManager,
            TcpSocketServer server)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (bufferManager == null)
                throw new ArgumentNullException("bufferManager");
            if (server == null)
                throw new ArgumentNullException("server");

            _tcpClient = tcpClient;
            _configuration = configuration;
            _bufferManager = bufferManager;
            _server = server;

            _sessionKey = Guid.NewGuid().ToString();
            this.StartTime = DateTime.UtcNow;

            SetSocketOptions();

            _remoteEndPoint = this.RemoteEndPoint;
            _localEndPoint = this.LocalEndPoint;
        }

        #endregion

        #region Properties

        public string SessionKey { get { return _sessionKey; } }
        public DateTime StartTime { get; private set; }
        public TimeSpan ConnectTimeout { get { return _configuration.ConnectTimeout; } }

        private bool Connected { get { return _tcpClient != null && _tcpClient.Connected; } }
        public IPEndPoint RemoteEndPoint { get { return Connected ? (IPEndPoint)_tcpClient.Client.RemoteEndPoint : _remoteEndPoint; } }
        public IPEndPoint LocalEndPoint { get { return Connected ? (IPEndPoint)_tcpClient.Client.LocalEndPoint : _localEndPoint; } }

        public Socket Socket { get { return Connected ? _tcpClient.Client : null; } }
        public Stream Stream { get { return _stream; } }
        public TcpSocketServer Server { get { return _server; } }

        public TcpSocketConnectionState State
        {
            get
            {
                switch (_state)
                {
                    case _none:
                        return TcpSocketConnectionState.None;
                    case _connecting:
                        return TcpSocketConnectionState.Connecting;
                    case _connected:
                        return TcpSocketConnectionState.Connected;
                    case _disposed:
                        return TcpSocketConnectionState.Closed;
                    default:
                        return TcpSocketConnectionState.Closed;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("SessionKey[{0}], RemoteEndPoint[{1}], LocalEndPoint[{2}]",
                this.SessionKey, this.RemoteEndPoint, this.LocalEndPoint);
        }

        #endregion

        #region Process

        internal void Start()
        {
            int origin = Interlocked.CompareExchange(ref _state, _connecting, _none);
            if (origin == _disposed)
            {
                throw new ObjectDisposedException("This tcp socket session has been disposed when connecting.");
            }
            else if (origin != _none)
            {
                throw new InvalidOperationException("This tcp socket session is in invalid state when connecting.");
            }

            try
            {
                _stream = NegotiateStream(_tcpClient.GetStream());

                if (_receiveBuffer == default(ArraySegment<byte>))
                    _receiveBuffer = _bufferManager.BorrowBuffer();
                _receiveBufferOffset = 0;

                if (Interlocked.CompareExchange(ref _state, _connected, _connecting) != _connecting)
                {
                    Close(false); // connecting with wrong state
                    throw new ObjectDisposedException("This tcp socket session has been disposed after connected.");
                }

                bool isErrorOccurredInUserSide = false;
                try
                {
                    _server.RaiseClientConnected(this);
                }
                catch (Exception ex) // catch all exceptions from out-side
                {
                    isErrorOccurredInUserSide = true;
                    HandleUserSideError(ex);
                }

                if (!isErrorOccurredInUserSide)
                {
                    ContinueReadBuffer();
                }
                else
                {
                    Close(true); // user side handle tcp connection error occurred
                }
            }
            catch (Exception ex) // catch exceptions then log then re-throw
            {
                _log.Error(ex.Message, ex);
                Close(true); // handle tcp connection error occurred
                throw;
            }
        }

        public void Close()
        {
            Close(true); // close by external
        }

        private void Close(bool shallNotifyUserSide)
        {
            if (Interlocked.Exchange(ref _state, _disposed) == _disposed)
            {
                return;
            }

            Shutdown();

            if (shallNotifyUserSide)
            {
                try
                {
                    _server.RaiseClientDisconnected(this);
                }
                catch (Exception ex) // catch all exceptions from out-side
                {
                    HandleUserSideError(ex);
                }
            }

            Clean();
        }

        public void Shutdown()
        {
            // The correct way to shut down the connection (especially if you are in a full-duplex conversation) 
            // is to call socket.Shutdown(SocketShutdown.Send) and give the remote party some time to close 
            // their send channel. This ensures that you receive any pending data instead of slamming the 
            // connection shut. ObjectDisposedException should never be part of the normal application flow.
            if (_tcpClient != null && _tcpClient.Connected)
            {
                _tcpClient.Client.Shutdown(SocketShutdown.Send);
            }
        }

        private void Clean()
        {
            try
            {
                try
                {
                    if (_stream != null)
                    {
                        _stream.Dispose();
                    }
                }
                catch { }
                try
                {
                    if (_tcpClient != null)
                    {
                        _tcpClient.Close();
                    }
                }
                catch { }
            }
            catch { }
            finally
            {
                _stream = null;
                _tcpClient = null;
            }

            if (_receiveBuffer != default(ArraySegment<byte>))
                _configuration.BufferManager.ReturnBuffer(_receiveBuffer);
            _receiveBuffer = default(ArraySegment<byte>);
            _receiveBufferOffset = 0;
        }

        private void SetSocketOptions()
        {
            _tcpClient.ReceiveBufferSize = _configuration.ReceiveBufferSize;
            _tcpClient.SendBufferSize = _configuration.SendBufferSize;
            _tcpClient.ReceiveTimeout = (int)_configuration.ReceiveTimeout.TotalMilliseconds;
            _tcpClient.SendTimeout = (int)_configuration.SendTimeout.TotalMilliseconds;
            _tcpClient.NoDelay = _configuration.NoDelay;
            _tcpClient.LingerState = _configuration.LingerState;

            if (_configuration.KeepAlive)
            {
                _tcpClient.Client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.KeepAlive,
                    (int)_configuration.KeepAliveInterval.TotalMilliseconds);
            }

            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, _configuration.ReuseAddress);
        }

        private Stream NegotiateStream(Stream stream)
        {
            if (!_configuration.SslEnabled)
                return stream;

            var validateRemoteCertificate = new RemoteCertificateValidationCallback(
                (object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
                =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    if (_configuration.SslPolicyErrorsBypassed)
                        return true;
                    else
                        _log.Error("Session [{0}] error occurred when validating remote certificate: [{1}], [{2}].",
                            this, this.RemoteEndPoint, sslPolicyErrors);

                    return false;
                });

            var sslStream = new SslStream(
                stream,
                false,
                validateRemoteCertificate,
                null,
                _configuration.SslEncryptionPolicy);

            IAsyncResult ar = null;
            if (!_configuration.SslClientCertificateRequired)
            {
                ar = sslStream.BeginAuthenticateAsServer(
                    _configuration.SslServerCertificate, // The X509Certificate used to authenticate the server.
                    null, _tcpClient);
            }
            else
            {
                ar = sslStream.BeginAuthenticateAsServer(
                    _configuration.SslServerCertificate, // The X509Certificate used to authenticate the server.
                    _configuration.SslClientCertificateRequired, // A Boolean value that specifies whether the client must supply a certificate for authentication.
                    _configuration.SslEnabledProtocols, // The SslProtocols value that represents the protocol used for authentication.
                    _configuration.SslCheckCertificateRevocation, // A Boolean value that specifies whether the certificate revocation list is checked during authentication.
                    null, _tcpClient);
            }
            if (!ar.AsyncWaitHandle.WaitOne(ConnectTimeout))
            {
                Close(false); // ssl negotiation timeout
                throw new TimeoutException(string.Format(
                    "Negotiate SSL/TSL with remote [{0}] timeout [{1}].", this.RemoteEndPoint, ConnectTimeout));
            }

            // When authentication succeeds, you must check the IsEncrypted and IsSigned properties 
            // to determine what security services are used by the SslStream. 
            // Check the IsMutuallyAuthenticated property to determine whether mutual authentication occurred.
            _log.Debug(
                "Ssl Stream: SslProtocol[{0}], IsServer[{1}], IsAuthenticated[{2}], IsEncrypted[{3}], IsSigned[{4}], IsMutuallyAuthenticated[{5}], "
                + "HashAlgorithm[{6}], HashStrength[{7}], KeyExchangeAlgorithm[{8}], KeyExchangeStrength[{9}], CipherAlgorithm[{10}], CipherStrength[{11}].",
                sslStream.SslProtocol,
                sslStream.IsServer,
                sslStream.IsAuthenticated,
                sslStream.IsEncrypted,
                sslStream.IsSigned,
                sslStream.IsMutuallyAuthenticated,
                sslStream.HashAlgorithm,
                sslStream.HashStrength,
                sslStream.KeyExchangeAlgorithm,
                sslStream.KeyExchangeStrength,
                sslStream.CipherAlgorithm,
                sslStream.CipherStrength);

            return sslStream;
        }

        private void ContinueReadBuffer()
        {
            try
            {
                _stream.BeginRead(
                    _receiveBuffer.Array,
                    _receiveBuffer.Offset + _receiveBufferOffset,
                    _receiveBuffer.Count - _receiveBufferOffset,
                    HandleDataReceived,
                    _stream);
            }
            catch (Exception ex) // catch exceptions then log then re-throw
            {
                HandleReceiveOperationException(ex);
                throw;
            }
        }

        private void HandleDataReceived(IAsyncResult ar)
        {
            if (State != TcpSocketConnectionState.Connected
                || _stream == null)
            {
                Close(false); // receive callback
                return;
            }

            try
            {
                int numberOfReadBytes = 0;
                try
                {
                    // The EndRead method blocks until data is available. The EndRead method reads 
                    // as much data as is available up to the number of bytes specified in the size 
                    // parameter of the BeginRead method. If the remote host shuts down the Socket 
                    // connection and all available data has been received, the EndRead method 
                    // completes immediately and returns zero bytes.
                    numberOfReadBytes = _stream.EndRead(ar);
                }
                catch (Exception)
                {
                    // unable to read data from transport connection, 
                    // the existing connection was forcibly closed by remote host
                    numberOfReadBytes = 0;
                }

                if (numberOfReadBytes == 0)
                {
                    Close(true); // receive 0-byte means connection has been closed
                    return;
                }

                ReceiveBuffer(numberOfReadBytes);

                ContinueReadBuffer();
            }
            catch (Exception ex)
            {
                try
                {
                    HandleReceiveOperationException(ex);
                }
                catch (Exception innnerException)
                {
                    _log.Error(innnerException.Message, innnerException);
                }
            }
        }

        private void ReceiveBuffer(int receiveCount)
        {
            // TCP guarantees delivery of all packets in the correct order. 
            // But there is no guarantee that one write operation on the sender-side will result in 
            // one read event on the receiving side. One call of write(message) by the sender 
            // can result in multiple messageReceived(session, message) events on the receiver; 
            // and multiple calls of write(message) can lead to a single messageReceived event.
            // In a stream-based transport such as TCP/IP, received data is stored into a socket receive buffer. 
            // Unfortunately, the buffer of a stream-based transport is not a queue of packets but a queue of bytes. 
            // It means, even if you sent two messages as two independent packets, 
            // an operating system will not treat them as two messages but as just a bunch of bytes. 
            // Therefore, there is no guarantee that what you read is exactly what your remote peer wrote.
            // There are three common techniques for splitting the stream of bytes into messages:
            //   1. use fixed length messages
            //   2. use a fixed length header that indicates the length of the body
            //   3. using a delimiter; for example many text-based protocols append
            //      a newline (or CR LF pair) after every message.
            int frameLength;
            byte[] payload;
            int payloadOffset;
            int payloadCount;
            int consumedLength = 0;

            SegmentBufferDeflector.ReplaceBuffer(_bufferManager, ref _receiveBuffer, ref _receiveBufferOffset, receiveCount);

            while (true)
            {
                frameLength = 0;
                payload = null;
                payloadOffset = 0;
                payloadCount = 0;

                if (_configuration.FrameBuilder.Decoder.TryDecodeFrame(
                    _receiveBuffer.Array,
                    _receiveBuffer.Offset + consumedLength,
                    _receiveBufferOffset - consumedLength,
                    out frameLength, out payload, out payloadOffset, out payloadCount))
                {
                    try
                    {
                        _server.RaiseClientDataReceived(this, payload, payloadOffset, payloadCount);
                    }
                    catch (Exception ex) // catch all exceptions from out-side
                    {
                        HandleUserSideError(ex);
                    }
                    finally
                    {
                        consumedLength += frameLength;
                    }
                }
                else
                {
                    break;
                }
            }

            if (_receiveBuffer != null && _receiveBuffer.Array != null)
            {
                SegmentBufferDeflector.ShiftBuffer(_bufferManager, consumedLength, ref _receiveBuffer, ref _receiveBufferOffset);
            }
        }

        #endregion

        #region Exception Handler

        private void HandleSendOperationException(Exception ex)
        {
            if (IsSocketTimeOut(ex))
            {
                CloseIfShould(ex);
                throw new TcpSocketException(ex.Message, new TimeoutException(ex.Message, ex));
            }

            CloseIfShould(ex);
            throw new TcpSocketException(ex.Message, ex);
        }

        private void HandleReceiveOperationException(Exception ex)
        {
            if (IsSocketTimeOut(ex))
            {
                CloseIfShould(ex);
                throw new TcpSocketException(ex.Message, new TimeoutException(ex.Message, ex));
            }

            CloseIfShould(ex);
            throw new TcpSocketException(ex.Message, ex);
        }

        private bool IsSocketTimeOut(Exception ex)
        {
            return ex is IOException
                && ex.InnerException != null
                && ex.InnerException is SocketException
                && (ex.InnerException as SocketException).SocketErrorCode == SocketError.TimedOut;
        }

        private bool CloseIfShould(Exception ex)
        {
            if (ex is ObjectDisposedException
                || ex is InvalidOperationException
                || ex is SocketException
                || ex is IOException
                || ex is NullReferenceException // buffer array operation
                || ex is ArgumentException      // buffer array operation
                )
            {
                if (ex is SocketException)
                    _log.Error(string.Format("Session [{0}] exception occurred, [{1}].", this, ex.Message), ex);

                Close(true); // catch specified exception then intend to close the session

                return true;
            }

            return false;
        }

        private void HandleUserSideError(Exception ex)
        {
            _log.Error(string.Format("Session [{0}] error occurred in user side [{1}].", this, ex.Message), ex);
        }

        #endregion

        #region Send

        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Send(data, 0, data.Length);
        }

        public void Send(byte[] data, int offset, int count)
        {
            BufferValidator.ValidateBuffer(data, offset, count, "data");

            if (State != TcpSocketConnectionState.Connected)
            {
                throw new InvalidProgramException("This session has been closed.");
            }

            try
            {
                byte[] frameBuffer;
                int frameBufferOffset;
                int frameBufferLength;
                _configuration.FrameBuilder.Encoder.EncodeFrame(data, offset, count, out frameBuffer, out frameBufferOffset, out frameBufferLength);

                _stream.Write(frameBuffer, frameBufferOffset, frameBufferLength);
            }
            catch (Exception ex) // catch exceptions then log then re-throw
            {
                HandleSendOperationException(ex);
                throw;
            }
        }

        public void BeginSend(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            BeginSend(data, 0, data.Length);
        }

        public void BeginSend(byte[] data, int offset, int count)
        {
            BufferValidator.ValidateBuffer(data, offset, count, "data");

            if (State != TcpSocketConnectionState.Connected)
            {
                throw new InvalidProgramException("This session has been closed.");
            }

            try
            {
                byte[] frameBuffer;
                int frameBufferOffset;
                int frameBufferLength;
                _configuration.FrameBuilder.Encoder.EncodeFrame(data, offset, count, out frameBuffer, out frameBufferOffset, out frameBufferLength);

                _stream.BeginWrite(frameBuffer, frameBufferOffset, frameBufferLength, HandleDataWritten, _stream);
            }
            catch (Exception ex) // catch exceptions then log then re-throw
            {
                HandleSendOperationException(ex);
                throw;
            }
        }

        private void HandleDataWritten(IAsyncResult ar)
        {
            try
            {
                if (_stream != null)
                {
                    _stream.EndWrite(ar);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    HandleSendOperationException(ex);
                }
                catch (Exception innnerException)
                {
                    _log.Error(innnerException.Message, innnerException);
                }
            }
        }

        public IAsyncResult BeginSend(byte[] data, AsyncCallback callback, object state)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return BeginSend(data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            BufferValidator.ValidateBuffer(data, offset, count, "data");

            if (State != TcpSocketConnectionState.Connected)
            {
                throw new InvalidProgramException("This session has been closed.");
            }

            try
            {
                byte[] frameBuffer;
                int frameBufferOffset;
                int frameBufferLength;
                _configuration.FrameBuilder.Encoder.EncodeFrame(data, offset, count, out frameBuffer, out frameBufferOffset, out frameBufferLength);

                return _stream.BeginWrite(frameBuffer, frameBufferOffset, frameBufferLength, callback, state);
            }
            catch (Exception ex) // catch exceptions then log then re-throw
            {
                HandleSendOperationException(ex);
                throw;
            }
        }

        public void EndSend(IAsyncResult asyncResult)
        {
            HandleDataWritten(asyncResult);
        }

        #endregion
    }
}
