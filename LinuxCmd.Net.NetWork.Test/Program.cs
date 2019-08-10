using System;

namespace LinuxCmd.Net.NetWork.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            NetWorker net = new NetWorker();
            net.Start();
            Console.ReadLine();
        }
    }
}
