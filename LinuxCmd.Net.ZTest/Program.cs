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

            //redirect:true 获取服务器命令执行结果
            var ls = "ls".LinuxBash().Output;

            //redirect:false 将命令执行结果重定向输出到服务器
            ls = "ls".LinuxBash(false).Output;

            //服务器状态对象
            LinuxServerInfo server = new LinuxServerInfo();

            //系统信息
            var OSName = server.OSName;
            $"echo {OSName}".LinuxBash(false);

            //运行时长
            var RunTime = server.RunTime;
            $"echo {RunTime}".LinuxBash(false);

            //系统负载
            var LoadAverages = server.LoadAverages;
            $"echo {LoadAverages}".LinuxBash(false);

            //CPU状态: cpu描述,cpu核心数,cpu使用率
            var cpuInfo = server.Cpu;
            $"echo {cpuInfo.SerializeJSON().Replace('(',' ').Replace(')',' ')}".LinuxBash(false);

            //内存状态:内存总容量,实际可用容量,已使用的容量,缓存化的容量,系统缓冲容量
            var Mem = server.Mem;
            $"echo {Mem.SerializeJSON()}".LinuxBash(false);

            //磁盘状态:磁盘总容量,已用容量,可用容量,已用百分比
            var Disk = server.Disk;
            $"echo {Disk.SerializeJSON()}".LinuxBash(false);

            //IO读写状态:读请求数量,写请求数量,读字节数,写字节数
            var IO = server.IO;
            $"echo {IO.SerializeJSON()}".LinuxBash(false);

            //网络状态:接收的数据包数量,发送的数据包数量,接收字节数,发送字节数
            var NetWork = server.NetWork;
            $"echo {NetWork.SerializeJSON()}".LinuxBash(false);

            //网络连接状态: tcp客户端IP,服务器IP,连接状态
            var NetworkConnections = server.NetworkConnections;
            foreach (var net in NetworkConnections)
            {
                $"echo {net.SerializeJSON()}".LinuxBash(false);
            }

            //进程列表:进程id,进程所有者的用户名,虚拟内存使用量,物理内存使用量,进程状态,CPU使用率,进程命令名
            var Tasks = server.Tasks;
            for (int i = 0; i < 6; i++)
            {
                $"echo {Tasks[i].SerializeJSON()}".LinuxBash(false);
            }

            

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
