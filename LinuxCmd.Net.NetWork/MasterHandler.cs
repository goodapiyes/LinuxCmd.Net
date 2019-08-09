using System;
using System.Threading;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.EventArgs;

namespace LinuxCmd.Net.NetWork
{
    public class MasterHandler : ServerHandlerBase
    {
        public string Ip { get; private set; }
        public int? Port { get; private set; }
        public IServer server;
        public Action<IServer, SessionReceiveEventArgs> Receive { get; set; }
        public MasterHandler()
        {
            this.Ip = ConfigHander.GetString("local:ip");
            this.Port = ConfigHander.GetInt("local:port");
        }

        public void Listening()
        {
            if (string.IsNullOrEmpty(Ip) || !Port.HasValue)
            {
                LogHelper.Logger.Error("ip,port is not available !!!");
                throw new Exception("ip,port is not available !!!");
            }
            server = SocketFactory.CreateTcpServer<MasterHandler>();
            server.Options.DefaultListen.Host = Ip;
            server.Options.DefaultListen.Port = Port.Value;
            server.Open();
            LogHelper.Logger.Information("Server Listening...");

            Thread.Sleep(20000);

        }

        public void SendToOnlines(string cmd, IServer server)
        {
            foreach (ISession item in server.GetOnlines())
            {
                item.Stream.ToPipeStream().WriteLine(cmd);
                item.Stream.Flush();
            }
        }

        public void Dispose()
        {
            server.Dispose();
        }
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            Receive?.Invoke(server, e);
            string line = e.Stream.ToPipeStream().ReadLine();
            LogHelper.Logger.Information("Server Receive Data From {0}@{1}: {2}",e.Session.RemoteEndPoint, e.Session.ID,line);
            e.Session.Stream.ToPipeStream().WriteLine("ok");
            e.Session.Stream.Flush();
            base.SessionReceive(server, e);
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server, e);
            LogHelper.Logger.Information("Session connected from {0}@{1}", e.Session.RemoteEndPoint, e.Session.ID);
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            base.Connecting(server, e);
            LogHelper.Logger.Information("Connect from {0}", e.Socket.RemoteEndPoint);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server, e);
            LogHelper.Logger.Information("Session {0}@{1} disconnected", e.Session.RemoteEndPoint, e.Session.ID);
        }

        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            base.Error(server, e);
            if (e.Session == null)
            {
                LogHelper.Logger.Information("Server error {0}@{1}\r\n{2}", e.Message, e.Error.Message, e.Error.StackTrace);
            }
            else
            {
                LogHelper.Logger.Information("Session {2}@{3} error {0}@{1}\r\n{4}", e.Message, e.Error.Message, e.Session.RemoteEndPoint, e.Session.ID, e.Error.StackTrace);
            }
        }

        public override void SessionDetection(IServer server, SessionDetectionEventArgs e)
        {
            base.SessionDetection(server, e);
        }

        protected override void OnReceiveMessage(IServer server, ISession session, object message)
        {
            base.OnReceiveMessage(server, session, message);
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            base.SessionPacketDecodeCompleted(server, e);
        }
    }
}