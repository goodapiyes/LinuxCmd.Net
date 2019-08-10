﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Cowboy.Sockets.Core
{
    public class TcpSocketSaeaServer : IDisposable
    {
        #region Fields

        private static readonly ILogger _log = LogHelper.Logger;
        private static readonly byte[] EmptyArray = new byte[0];
        private readonly ConcurrentDictionary<string, TcpSocketSaeaSession> _sessions = new ConcurrentDictionary<string, TcpSocketSaeaSession>();
        private readonly TcpSocketSaeaServerConfiguration _configuration;
        private readonly ITcpSocketSaeaServerEventDispatcher _dispatcher;

        private int _state;
        private const int _none = 0;
        private const int _listening = 1;
        private const int _disposed = 5;

        private Socket _listener;
        private SaeaPool _acceptSaeaPool;
        private SaeaPool _handleSaeaPool;
        private SessionPool _sessionPool;

        private readonly object _disposeLock = new object();
        private bool _isDisposed;

        #endregion

        #region Constructors

        public TcpSocketSaeaServer(int listenedPort, ITcpSocketSaeaServerEventDispatcher dispatcher, TcpSocketSaeaServerConfiguration configuration = null)
            : this(IPAddress.Any, listenedPort, dispatcher, configuration)
        {
        }

        public TcpSocketSaeaServer(IPAddress listenedAddress, int listenedPort, ITcpSocketSaeaServerEventDispatcher dispatcher, TcpSocketSaeaServerConfiguration configuration = null)
            : this(new IPEndPoint(listenedAddress, listenedPort), dispatcher, configuration)
        {
        }

        public TcpSocketSaeaServer(IPEndPoint listenedEndPoint, ITcpSocketSaeaServerEventDispatcher dispatcher, TcpSocketSaeaServerConfiguration configuration = null)
        {
            if (listenedEndPoint == null)
                throw new ArgumentNullException("listenedEndPoint");
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            this.ListenedEndPoint = listenedEndPoint;
            _dispatcher = dispatcher;
            _configuration = configuration ?? new TcpSocketSaeaServerConfiguration();

            if (_configuration.BufferManager == null)
                throw new InvalidProgramException("The buffer manager in configuration cannot be null.");
            if (_configuration.FrameBuilder == null)
                throw new InvalidProgramException("The frame handler in configuration cannot be null.");

            Initialize();
        }

        public TcpSocketSaeaServer(
            int listenedPort,
            Func<TcpSocketSaeaSession, byte[], int, int, Task> onSessionDataReceived = null,
            Func<TcpSocketSaeaSession, Task> onSessionStarted = null,
            Func<TcpSocketSaeaSession, Task> onSessionClosed = null,
            TcpSocketSaeaServerConfiguration configuration = null)
            : this(IPAddress.Any, listenedPort, onSessionDataReceived, onSessionStarted, onSessionClosed, configuration)
        {
        }

        public TcpSocketSaeaServer(
            IPAddress listenedAddress, int listenedPort,
            Func<TcpSocketSaeaSession, byte[], int, int, Task> onSessionDataReceived = null,
            Func<TcpSocketSaeaSession, Task> onSessionStarted = null,
            Func<TcpSocketSaeaSession, Task> onSessionClosed = null,
            TcpSocketSaeaServerConfiguration configuration = null)
            : this(new IPEndPoint(listenedAddress, listenedPort), onSessionDataReceived, onSessionStarted, onSessionClosed, configuration)
        {
        }

        public TcpSocketSaeaServer(
            IPEndPoint listenedEndPoint,
            Func<TcpSocketSaeaSession, byte[], int, int, Task> onSessionDataReceived = null,
            Func<TcpSocketSaeaSession, Task> onSessionStarted = null,
            Func<TcpSocketSaeaSession, Task> onSessionClosed = null,
            TcpSocketSaeaServerConfiguration configuration = null)
            : this(listenedEndPoint,
                  new DefaultTcpSocketSaeaServerEventDispatcher(onSessionDataReceived, onSessionStarted, onSessionClosed),
                  configuration)
        {
        }

        private void Initialize()
        {
            _acceptSaeaPool = new SaeaPool(
                () =>
                {
                    var saea = new SaeaAwaitable();
                    return saea;
                },
                (saea) =>
                {
                    try
                    {
                        saea.Saea.AcceptSocket = null;
                        saea.Saea.SetBuffer(0, 0);
                        saea.Saea.RemoteEndPoint = null;
                        saea.Saea.SocketFlags = SocketFlags.None;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                })
                .Initialize(16);
            _handleSaeaPool = new SaeaPool(
                () =>
                {
                    var saea = new SaeaAwaitable();
                    return saea;
                },
                (saea) =>
                {
                    try
                    {
                        saea.Saea.AcceptSocket = null;
                        saea.Saea.SetBuffer(EmptyArray, 0, 0);
                        saea.Saea.RemoteEndPoint = null;
                        saea.Saea.SocketFlags = SocketFlags.None;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                })
                .Initialize(1024);
            _sessionPool = new SessionPool(
                () =>
                {
                    var session = new TcpSocketSaeaSession(_configuration, _configuration.BufferManager, _handleSaeaPool, _dispatcher, this);
                    return session;
                },
                (session) =>
                {
                    try
                    {
                        session.Detach();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                })
                .Initialize(512);
        }

        #endregion

        #region Properties

        public IPEndPoint ListenedEndPoint { get; private set; }
        public bool IsListening { get { return _state == _listening; } }
        public int SessionCount { get { return _sessions.Count; } }

        #endregion

        #region Server

        public void Listen()
        {
            int origin = Interlocked.CompareExchange(ref _state, _listening, _none);
            if (origin == _disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            else if (origin != _none)
            {
                throw new InvalidOperationException("This tcp server has already started.");
            }

            try
            {
                _listener = new Socket(this.ListenedEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SetSocketOptions();

                _listener.Bind(this.ListenedEndPoint);

                _listener.Listen(_configuration.PendingConnectionBacklog);

                Task.Factory.StartNew(async () =>
                {
                    await Accept();
                },
                TaskCreationOptions.LongRunning)
                .Forget();
            }
            catch (Exception ex) when (!ShouldThrow(ex)) { }
        }

        public void Shutdown()
        {
            if (Interlocked.Exchange(ref _state, _disposed) == _disposed)
            {
                return;
            }

            try
            {
                _listener.Close(0);
                _listener = null;

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        foreach (var session in _sessions.Values)
                        {
                            await session.Close(); // parent server close session when shutdown
                        }
                    }
                    catch (Exception ex) when (!ShouldThrow(ex)) { }
                },
                TaskCreationOptions.PreferFairness)
                .Wait();
            }
            catch (Exception ex) when (!ShouldThrow(ex)) { }
        }

        private void SetSocketOptions()
        {
            if (_configuration.AllowNatTraversal)
            {
                _listener.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            else
            {
                _listener.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
            }
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, _configuration.ReuseAddress);
        }

        private bool ShouldThrow(Exception ex)
        {
            if (ex is ObjectDisposedException
                || ex is InvalidOperationException
                || ex is SocketException
                || ex is IOException)
            {
                return false;
            }
            return true;
        }

        private async Task Accept()
        {
            try
            {
                while (IsListening)
                {
                    var saea = _acceptSaeaPool.Take();

                    var socketError = await _listener.AcceptAsync(saea);
                    if (socketError == SocketError.Success)
                    {
                        var acceptedSocket = saea.Saea.AcceptSocket;
                        Task.Factory.StartNew(async () =>
                        {
                            await Process(acceptedSocket);
                        },
                        TaskCreationOptions.None)
                        .Forget();
                    }
                    else
                    {
                        _log.Error("Error occurred when accept incoming socket [{0}].", socketError);
                    }

                    _acceptSaeaPool.Return(saea);
                }
            }
            catch (Exception ex) when (!ShouldThrow(ex)) { }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private async Task Process(Socket acceptedSocket)
        {
            var session = _sessionPool.Take();
            session.Attach(acceptedSocket);

            if (_sessions.TryAdd(session.SessionKey, session))
            {
                _log.Debug("New session [{0}].", session);
                try
                {
                    await session.Start();
                }
                finally
                {
                    TcpSocketSaeaSession recycle;
                    if (_sessions.TryRemove(session.SessionKey, out recycle))
                    {
                        _log.Debug("Close session [{0}].", recycle);
                    }
                }
            }

            _sessionPool.Return(session);
        }

        #endregion

        #region Send

        public async Task SendToAsync(string sessionKey, byte[] data)
        {
            await SendToAsync(sessionKey, data, 0, data.Length);
        }

        public async Task SendToAsync(string sessionKey, byte[] data, int offset, int count)
        {
            TcpSocketSaeaSession sessionFound;
            if (_sessions.TryGetValue(sessionKey, out sessionFound))
            {
                await sessionFound.SendAsync(data, offset, count);
            }
            else
            {
                _log.Warning("Cannot find session [{0}].", sessionKey);
            }
        }

        public async Task SendToAsync(TcpSocketSaeaSession session, byte[] data)
        {
            await SendToAsync(session, data, 0, data.Length);
        }

        public async Task SendToAsync(TcpSocketSaeaSession session, byte[] data, int offset, int count)
        {
            TcpSocketSaeaSession sessionFound;
            if (_sessions.TryGetValue(session.SessionKey, out sessionFound))
            {
                await sessionFound.SendAsync(data, offset, count);
            }
            else
            {
                _log.Warning("Cannot find session [{0}].", session);
            }
        }

        public async Task BroadcastAsync(byte[] data)
        {
            await BroadcastAsync(data, 0, data.Length);
        }

        public async Task BroadcastAsync(byte[] data, int offset, int count)
        {
            foreach (var session in _sessions.Values)
            {
                await session.SendAsync(data, offset, count);
            }
        }

        #endregion

        #region Session

        public bool HasSession(string sessionKey)
        {
            return _sessions.ContainsKey(sessionKey);
        }

        public TcpSocketSaeaSession GetSession(string sessionKey)
        {
            TcpSocketSaeaSession session = null;
            _sessions.TryGetValue(sessionKey, out session);
            return session;
        }

        public async Task CloseSession(string sessionKey)
        {
            TcpSocketSaeaSession session = null;
            if (_sessions.TryGetValue(sessionKey, out session))
            {
                await session.Close(); // parent server close session by session-key
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (_isDisposed)
                    return;

                if (disposing)
                {
                    // free managed objects here
                    try
                    {
                        if (_listener != null)
                            _listener.Dispose();
                        if (_acceptSaeaPool != null)
                            _acceptSaeaPool.Dispose();
                        if (_handleSaeaPool != null)
                            _handleSaeaPool.Dispose();
                        if (_sessionPool != null)
                            _sessionPool.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
