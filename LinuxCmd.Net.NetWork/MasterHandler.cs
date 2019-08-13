using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cowboy.Sockets.Core;

namespace LinuxCmd.Net.NetWork
{
    public class MasterHandler
    {
        public string Ip { get; private set; }
        public int? Port { get; private set; }
        public TcpSocketServer Server;
        public ConcurrentDictionary<string, TcpSocketSession> Sessions { get; private set; }
        public MasterHandler()
        {
            this.Ip = ConfigHander.GetString("master:ip");
            this.Port = ConfigHander.GetInt("master:port");
            Sessions=new ConcurrentDictionary<string, TcpSocketSession>();
        }

        public void Listening()
        {
            if (string.IsNullOrEmpty(Ip) || !Port.HasValue)
            {
                LogHelper.Logger.Error("ip,port is not available !!!");
                throw new Exception("ip,port is not available !!!");
            }

            var config = new TcpSocketServerConfiguration();
            Server = new TcpSocketServer(IPAddress.Parse(Ip), Port.Value, config);
            Server.ClientConnected += ClientConnected;
            Server.ClientDisconnected += ClientDisconnected;
            Server.ClientDataReceived += ClientDataReceived;
            Server.Listen();
            LogHelper.Logger.Information("Server Listening...");
        }

        public bool Issue(string ip,string cmd)
        {
            try
            {
                foreach (var session in Sessions)
                {
                    if(session.Key.Contains(ip))
                        Server.SendTo(session.Value.SessionKey,Encoding.UTF8.GetBytes(cmd));
                }
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error($"Issue {ip},Error:{e.Message},{e.StackTrace}");
                return false;
            }
            return true;
        }

        private void ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            
            var key = e.Session.RemoteEndPoint.ToString();
            Sessions.AddOrUpdate(key, e.Session, (k, o) => o);
            Console.WriteLine($"客户端 {e.Session.RemoteEndPoint} 已连接 {e.Session}.");
        }

        private void ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            var key = e.Session.RemoteEndPoint.ToString();
            TcpSocketSession removeSession;
            if(Sessions.ContainsKey(key))
                Sessions.TryRemove(key, out removeSession);
            Console.WriteLine($"客户端 {e.Session} 关闭了连接.");
        }

        private void ClientDataReceived(object sender, TcpClientDataReceivedEventArgs e)
        {
            var text = Encoding.UTF8.GetString(e.Data, e.DataOffset, e.DataLength);
            Console.Write(string.Format("客户端 : {0} {1} --> ", e.Session.RemoteEndPoint, e.Session));
            if (e.DataLength < 256)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.WriteLine("{0} Bytes", e.DataLength);
            }
            Server.SendTo(e.Session, Encoding.UTF8.GetBytes(text));
        }

        public void Dispose()
        {
            Server.Shutdown();
            Server = null;
        }
    }
}