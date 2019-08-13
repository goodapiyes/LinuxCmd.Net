using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace LinuxCmd.Net.NetWork
{
    public class NetWorker 
    {
        public MasterHandler Master { get; private set; }
        public ClientHandler Client { get; private set; }

        public void Start()
        {
            if (ConfigHander.GetString("type") == "master")
                InitMaster();
            else if (ConfigHander.GetString("type") == "worker")
                InitClient();
        }
        private void InitMaster()
        {
            Master = new MasterHandler();
            Master.Listening();
        }

        private void InitClient()
        {
            Client =new ClientHandler();
            Client.Start();
        }
        
        public bool Dispose()
        {
            if (ConfigHander.GetString("type") == "master")
                Master.Dispose();
            else if (ConfigHander.GetString("type") == "worker")
                Client.Dispose();
            return true;
        }
    }
}