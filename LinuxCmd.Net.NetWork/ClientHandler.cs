using System;
using BeetleX;
using BeetleX.Clients;

namespace LinuxCmd.Net.NetWork
{
    public class ClientHandler
    {
        public string Ip { get; private set; }
        public int? Port { get; private set; }
        public int? ReconnectionCount { get; private set; }
        public int? TimeOut { get; private set; }
        public int? Polling { get; private set; }
        public int Reconnectioned { get; private set; }
        public System.Threading.Timer PollingTimer { get; private set; }
        public AsyncTcpClient TcpClient;
        public bool downTag = false;
        public ClientHandler()
        {
            LoadConfig();
        }

        public void Start()
        {
            LogHelper.Logger.Information("Client Start...");
            CreateTcpClient();
            PollingTimer = new System.Threading.Timer(Heartbeat, this, Polling.Value * 1000, Polling.Value * 1000);
        }

        private void LoadConfig()
        {
            ConfigHander.configuration.Reload();
            this.Ip = ConfigHander.GetString("target:ip");
            this.Port = ConfigHander.GetInt("target:port");
            this.ReconnectionCount = ConfigHander.GetInt("local:reconnection");
            this.TimeOut = ConfigHander.GetInt("local:timeout");
            this.Polling = ConfigHander.GetInt("local:polling");
        }

        private void CreateTcpClient()
        {
            if (string.IsNullOrEmpty(Ip) || !Port.HasValue)
            {
                LogHelper.Logger.Error("ip,port is not available !!!");
                throw new Exception("ip,port is not available !!");
            }
            TcpClient = SocketFactory.CreateClient<AsyncTcpClient>(Ip, Port.Value);
            if (TimeOut != null) TcpClient.TimeOut = TimeOut.Value * 1000;
            TcpClient.DataReceive = (o, e) =>
            {
                string line = e.Stream.ToPipeStream().ReadLine();
                LogHelper.Logger.Information($"Receive Data: {line}");
            };
            TcpClient.ClientError = (c, e) =>
            {
                LogHelper.Logger.Error($"TcpClient Error:{e.Message},{e.Error.StackTrace}");
            };
        }
        private void Heartbeat(object obj)
        {
            if (Reconnectioned >= ReconnectionCount)
            {
                //重连熔断降级至24小时一次轮询
                if (!downTag)
                {
                    LogHelper.Logger.Error($"{ReconnectionCount}次重连失败,降级轮询频率至24小时一次");
                    PollingTimer.Change(24 * 60 * 60 * 1000, 24 * 60 * 60 * 1000);
                }
                return;
            }



            if (TcpClient == null)
                CreateTcpClient();
            if (TcpClient.Connect())
            {
                TcpClient.Stream.ToPipeStream().WriteLine(CommandHandler.HeartBeatCmd());
                TcpClient.Stream.Flush();
                Reconnectioned = 0;
                //恢复轮询正常状态
                downTag = true;
                PollingTimer.Change(Polling.Value * 1000, Polling.Value * 1000);
            }
            else
            {
                TcpClient.DisConnect();
                TcpClient.LocalEndPoint = null;
                Reconnectioned++;
            }
        }
        public void Dispose()
        {
            TcpClient.DisConnect();
            TcpClient.Dispose();
            PollingTimer.Dispose();
        }
    }
}