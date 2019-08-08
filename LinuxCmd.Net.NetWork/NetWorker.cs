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
        private TcpClient client;
        private System.Threading.Timer timer;
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
                var ip = configuration["master:ip"];
                var port = Convert.ToInt32(configuration["master:port"]);
                client = SocketFactory.CreateClient<TcpClient>(ip, port);
                client.TimeOut = 60 * 1000;
                timer = new System.Threading.Timer(DoCall, this, 10000, 0);
                
            }

            return true;
        }
        private void DoCall(object obj)
        {
            if (client != null)
            {
                client.Stream.ToPipeStream().WriteLine("hello server !!!");
                client.Stream.Flush();
                var reader = client.Receive();
                var result = reader.ToPipeStream().ReadLine();
                Console.WriteLine(result);
            }
        }
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            string name = e.Stream.ToPipeStream().ReadLine();
            Console.WriteLine(name);
            e.Session.Stream.ToPipeStream().WriteLine("hello " + e.Session.Host);
            e.Session.Stream.Flush();
            base.SessionReceive(server, e);
        }

    }
}