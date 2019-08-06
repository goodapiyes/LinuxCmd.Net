using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LinuxCmd.Net;
using LinuxCmd.Net.Commads;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.ZTest
{
    class Program
    {
        //you can’t get to run on Windows
        static void Main(string[] args)
        {


            //string text = File.ReadAllText("text.txt");

            //redirect:true Gets the execution result of the server
            var ls = "ls".LinuxBash().Output;

            //redirect:false The result is output to the server
            ls = "ls".LinuxBash(false).Output;

            //Get linux server status, include CPU, memory, and networking. use top cmd
            LinuxTopInfo top = LinuxHelper.LinuxTop();

            //The result is output to the server
            $"echo {top.TaskDetails[0].SerializeJSON()}".LinuxBash(false);

            //cpu usage
            var cpu = top.UserCpu;

            //mem usage
            var mem = top.MemUsed;

            //server load
            var averages = top.Averages;

            //The process list
            var taskList = top.TaskDetails;

            //Disk status
            LinuxDfInfo disk = LinuxHelper.LinuxDisk();
            $"echo {disk.SerializeJSON()}".LinuxBash(false);

            //Disk read/write rate
            LinuxVmstatInfo vmstat = LinuxHelper.LinuxVmstat();
            $"echo {vmstat.SerializeJSON()}".LinuxBash(false);

            //Network packets
            LinuxSarInfo sar = LinuxHelper.LinuxSar();
            $"echo {sar.SerializeJSON()}".LinuxBash(false);

            //Tcp Network Connections
            var netstats = LinuxHelper.LinuxNetstats();
            foreach (var net in netstats)
            {
                $"echo {net.SerializeJSON()}".LinuxBash(false);
            }

            LinuxServerInfo server = new LinuxServerInfo();
            //Server status
            var OSName = server.OSName;
            $"echo {OSName}".LinuxBash(false);

            var RunTime = server.RunTime;
            $"echo {RunTime}".LinuxBash(false);

            var CpuCores = server.CpuCores;
            $"echo {CpuCores}".LinuxBash(false);

            var CpuUsage = server.CpuUsage;
            $"echo {CpuUsage}".LinuxBash(false);

            var MemTotal = server.MemTotal;
            $"echo {MemTotal}".LinuxBash(false);

            var MemAvailable = server.MemAvailable;
            $"echo {MemAvailable}".LinuxBash(false);

            var MemUsed = server.MemUsed;
            $"echo {MemUsed}".LinuxBash(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }

    public static class JsonHelper
    {

        public static string SerializeJSON<T>(this T data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

         public static T DeserializeJSON<T>(this string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

    }
}
