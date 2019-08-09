using System;
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
        }

        public void Dispose()
        {
            server.Dispose();
        }
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            Receive?.Invoke(server, e);
            string line = e.Stream.ToPipeStream().ReadLine();
            LogHelper.Logger.Information("Server Receive Data: "+line);
            e.Session.Stream.ToPipeStream().WriteLine("hello " + e.Server.GetOnlines().Length);
            e.Session.Stream.Flush();
            base.SessionReceive(server, e);
        }

    }
}