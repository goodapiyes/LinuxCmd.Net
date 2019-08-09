using System;
using System.IO;
using System.Threading;
using BeetleX;
using BeetleX.Clients;
using BeetleX.EventArgs;
using Microsoft.Extensions.Configuration;

namespace LinuxCmd.Net.NetWork
{
    public class NetWorker 
    {
        public MasterHandler Master { get; private set; }
        public ClientHandler Client { get; private set; }

        public void Start()
        {
            if (ConfigHander.GetInt("local:isMaster") == 1)
                InitMaster();
            else
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
            if (ConfigHander.GetInt("local:isMaster") == 1)
                Master.Dispose();
            else
                Client.Dispose();
            return true;
        }
    }
}