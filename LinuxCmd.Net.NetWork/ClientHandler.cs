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
        public int? TimeOut { get;  }
        public int? Polling { get; private set; }
        public int Reconnectioned { get; private set; }
        public System.Threading.Timer PollingTimer { get; private set; }
        public AsyncTcpClient TcpClient;

        public ClientHandler()
        {
            this.Ip= ConfigHander.GetString("target:ip");
            this.Port= ConfigHander.GetInt("target:port");
            this.ReconnectionCount= ConfigHander.GetInt("local:reconnection");
            this.TimeOut = ConfigHander.GetInt("local:timeout");
            this.Polling = ConfigHander.GetInt("local:polling");
        }

        public void Start()
        {
            CreateTcpClient();
            PollingTimer = new System.Threading.Timer(Heartbeat, this, Polling.Value * 1000, Polling.Value * 1000);
        }

        private void CreateTcpClient()
        {
            if (string.IsNullOrEmpty(Ip) || !Port.HasValue)
                throw new Exception("ip,port is not available !!");
            TcpClient = SocketFactory.CreateClient<AsyncTcpClient>(Ip, Port.Value);
            if (TimeOut != null) TcpClient.TimeOut = TimeOut.Value * 1000;
            TcpClient.DataReceive = (o, e) =>
            {
                string line = e.Stream.ToPipeStream().ReadLine();
                Console.WriteLine(line);
            };
        }
        private void Heartbeat(object obj)
        {
            Console.WriteLine(Reconnectioned);
            if (Reconnectioned >= ReconnectionCount)
                return;
            if (TcpClient == null)
                CreateTcpClient();
            if (TcpClient.Connect())
            {
                TcpClient.Stream.ToPipeStream().WriteLine("heartbeat packet");
                TcpClient.Stream.Flush();
            }
            else
            {
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