using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.Commads
{
    /// <summary>
    /// top version 3.3.10
    /// </summary>
    public class Top
    {
        public virtual LinuxTopInfo GetTop(string input)
        {
            var rows = input.Split("\n");
            if (rows.Length < 5)
                return null;

            LinuxTopInfo info=new LinuxTopInfo();

            //统计时间,服务器负载
            var stas = rows[0];
            info.Time = Utility.GetSingleByRgx(stas, @"top\s+-\s+(\S+)\s+up");
            info.Days = Convert.ToInt32(Utility.GetSingleByRgx(stas, @"(\d+)\s+days"));
            info.Users = Convert.ToInt32(Utility.GetSingleByRgx(stas, @"(\d+)\s+user"));
            info.Averages = $"{Utility.GetSingleByRgx(stas, @":\s+(\d+\.\d\d),")},{Utility.GetSingleByRgx(stas, @",\s+(\d+\.\d\d),")},{Utility.GetSingleByRgx(stas, @",\s+(\d+\.\d\d)")}";
            
            //进程统计
            var tasks = rows[1];
            info.TaskTotal = Convert.ToInt32(Utility.GetSingleByRgx(tasks, @"(\d+)\s+total"));
            info.RunningCount = Convert.ToInt32(Utility.GetSingleByRgx(tasks, @"(\d+)\s+running"));
            info.SleepingCount = Convert.ToInt32(Utility.GetSingleByRgx(tasks, @"(\d+)\s+sleeping"));
            info.StoppedCount = Convert.ToInt32(Utility.GetSingleByRgx(tasks, @"(\d+)\s+stopped"));
            info.ZombieCount = Convert.ToInt32(Utility.GetSingleByRgx(tasks, @"(\d+)\s+zombie"));

            //cpu数值
            var cpus = rows[2];
            info.UserCpu = Convert.ToDouble(Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+us"));
            info.SystemCpu = Convert.ToDouble(Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+sy"));
            info.NiCpu = Convert.ToDouble(Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+ni"));
            info.FreeCpu = Convert.ToDouble(Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+id"));
            info.WaitCpu = Convert.ToDouble(Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+wa"));
            var hi = Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+hi");
            var si = Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+si");
            var st = Utility.GetSingleByRgx(cpus, @"(\d+\.\d+)\s+st");

            //内存数值
            var mems = rows[3];
            info.MemTotal = Convert.ToDouble(Utility.GetSingleByRgx(mems, @"(\d+)\s+total")) / 1024;
            info.MemFree = Convert.ToDouble(Utility.GetSingleByRgx(mems, @"(\d+)\s+free")) / 1024;
            info.MemUsed = Convert.ToDouble(Utility.GetSingleByRgx(mems, @"(\d+)\s+used")) / 1024;
            info.MemCache = Convert.ToDouble(Utility.GetSingleByRgx(mems, @"(\d+)\s+buff/cache")) / 1024;

            //虚拟内存数值
            var swap = rows[4];
            info.SwapTotal = Convert.ToDouble(Utility.GetSingleByRgx(swap, @"(\d+)\s+total")) / 1024;
            info.SwapFree = Convert.ToDouble(Utility.GetSingleByRgx(swap, @"(\d+)\s+free")) / 1024;
            info.SwapUsed = Convert.ToDouble(Utility.GetSingleByRgx(swap, @"(\d+)\s+used")) / 1024;
            info.SwapCache = Convert.ToDouble(Utility.GetSingleByRgx(swap, @"(\d+)\s+avail")) / 1024;
            info.TaskDetails= GetTaskDetailsByRgx(input);
            return info;
        }

        

        protected virtual List<LinuxTopInfo_TaskDetail> GetTaskDetailsByRgx(string input)
        {
            List<LinuxTopInfo_TaskDetail> details= new List<LinuxTopInfo_TaskDetail>();
            var matchs = Regex.Matches(input, @"\s+(\d+)\s+(\S+)\s+\d+\s+\d+\s+(\d+)\s+(\d+)\s+(\d+)\s+(\w+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s*");
            foreach (Match match in matchs)
            {
                if (match.Success)
                {
                    var detail = new LinuxTopInfo_TaskDetail();
                    detail.Pid = match.Groups[1].Value;
                    detail.User = match.Groups[2].Value;
                    detail.Virt = Convert.ToDouble(match.Groups[3].Value) / 1024;
                    detail.Res = Convert.ToDouble(match.Groups[4].Value) / 1024;
                    detail.Shr = Convert.ToDouble(match.Groups[5].Value) / 1024;
                    detail.Status = match.Groups[6].Value;
                    detail.Cpu = Convert.ToDouble(match.Groups[7].Value);
                    detail.Mem = Convert.ToDouble(match.Groups[8].Value);
                    detail.Time = match.Groups[9].Value;
                    detail.Commad = match.Groups[10].Value;
                    details.Add(detail);
                }
            }
            return details;
        }
    }
}
