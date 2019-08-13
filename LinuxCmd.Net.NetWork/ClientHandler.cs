using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cowboy.Sockets.Core;

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
        public TcpSocketClient Client;
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
            this.Ip = ConfigHander.GetString("master:ip");
            this.Port = ConfigHander.GetInt("master:port");
            this.ReconnectionCount = ConfigHander.GetInt("worker:reconnection");
            this.TimeOut = ConfigHander.GetInt("worker:timeout");
            this.Polling = ConfigHander.GetInt("worker:polling");
        }

        private void CreateTcpClient()
        {
            if (string.IsNullOrEmpty(Ip) || !Port.HasValue)
            {
                LogHelper.Logger.Error("ip,port is not available !!!");
                throw new Exception("ip,port is not available !!");
            }

            var config = new TcpSocketClientConfiguration();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port.Value);

            Client = new TcpSocketClient(ipEndPoint, config);
            Client.ServerConnected += ServerConnected;
            Client.ServerDisconnected += ServerDisconnected;
            Client.ServerDataReceived += ServerDataReceived;
            Client.Connect();
        }
        private void Heartbeat(object obj)
        {
            try
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

                if (Client == null)
                    CreateTcpClient();
                if (Client.State == TcpSocketConnectionState.Connected)
                {
                    Client.Send(Encoding.UTF8.GetBytes(CommandHandler.HeartBeatCmd()));
                    Reconnectioned = 0;
                    //恢复轮询正常状态
                    downTag = true;
                    PollingTimer.Change(Polling.Value * 1000, Polling.Value * 1000);
                }
                else
                {
                    Reconnectioned++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error($"Heartbeat Error:"+ex.Message+ex.StackTrace);
            }
        }

        private void ServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            Console.WriteLine($"服务器 {e.RemoteEndPoint} 已连接.");
        }

        private void ServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            Console.WriteLine($"服务器 {e.RemoteEndPoint} 断开连接.");
        }

        private void ServerDataReceived(object sender, TcpServerDataReceivedEventArgs e)
        {
            var text = Encoding.UTF8.GetString(e.Data, e.DataOffset, e.DataLength);
            Console.Write($"服务器 : {e.Client.RemoteEndPoint} --> ");
            if (e.DataLength < 256)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.WriteLine("{0} Bytes", e.DataLength);
            }
        }

        public void Dispose()
        {
            Client.Close();
            Client.Dispose();
            Client = null;
            PollingTimer.Dispose();
            PollingTimer = null;
        }
    }
}