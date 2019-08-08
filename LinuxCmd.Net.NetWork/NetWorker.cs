using System;
using System.IO;
using System.Threading;
using BeetleX;
using BeetleX.Clients;
using BeetleX.EventArgs;
using Microsoft.Extensions.Configuration;

namespace LinuxCmd.Net.NetWork
{
    public class NetWorker : ServerHandlerBase
    {
        private IConfigurationRoot configuration;
        private IServer server;
        private AsyncTcpClient client;
        private System.Threading.Timer timer;
        public int Reconnectioned { get; set; } = 0;
        public NetWorker()
        {
            if(!File.Exists($"{AppContext.BaseDirectory}/app.json"))
                throw new Exception("Configuration file not found!!!");

            configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("app.json")
                .Build();

        }
        public bool Start()
        {
            if (configuration["local:isMaster"] == "1")
            {
                var ip = configuration["local:ip"];
                var port = Convert.ToInt32(configuration["local:port"]);
                server = SocketFactory.CreateTcpServer<NetWorker>();
                server.Options.DefaultListen.Host = ip;
                server.Options.DefaultListen.Port = port;
                server.Open();
            }
            else
            {
                timer = new System.Threading.Timer(Heartbeat, this, 10 * 1000, 10 * 1000);
            }

            return true;
        }
        public bool CreateClient()
        {

            var ip = configuration["master:ip"];
            var port = Convert.ToInt32(configuration["master:port"]);
            client = SocketFactory.CreateClient<AsyncTcpClient>(ip, port);
            client.TimeOut = 60 * 1000;
            client.DataReceive = (o, e) =>
            {
                string line = e.Stream.ToPipeStream().ReadLine();
                Console.WriteLine(line);
            };
            return true;
        }
        private void Heartbeat(object obj)
        {
            configuration.Reload();



            var reconnection = Convert.ToInt32(configuration["local:reconnection"]);
            if (Reconnectioned >= reconnection)
                return;

            if (client == null)
                CreateClient();
            if (client.Connect())
            {
                client.Stream.ToPipeStream().WriteLine("heartbeat packet");
                client.Stream.Flush();
            }
            else
            {
                Reconnectioned++;
            }
        }
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            string name = e.Stream.ToPipeStream().ReadLine();
            Console.WriteLine(name);
            e.Session.Stream.ToPipeStream().WriteLine("hello " + e.Server.GetOnlines().Length);
            e.Session.Stream.Flush();
            base.SessionReceive(server, e);
        }
        public bool Dispose()
        {
            server.Dispose();
            client.DisConnect();
            client.Dispose();
            timer.Dispose();
            return true;
        }
    }
}