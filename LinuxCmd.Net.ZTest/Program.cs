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
            //var output = (new Vmstat()).GetLinuxVmstatInfo(text);
            //return;
            //top version 3.3.10
            //string top = "top -b -n 1".LinuxBash();
            //string top = File.ReadAllText("top.txt");
            LinuxTopInfo info = LinuxHelper.LinuxTop();
            $"echo {info.TaskDetails[0].SerializeJSON()}".LinuxBash(false);
            var lsResult = "ls".LinuxBash(false);
            LinuxDfInfo disk = LinuxHelper.LinuxDisk();
            $"echo {disk.SerializeJSON()}".LinuxBash(false);
            LinuxVmstatInfo vmstat = LinuxHelper.LinuxVmstat();
            $"echo {vmstat.SerializeJSON()}".LinuxBash(false);
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
