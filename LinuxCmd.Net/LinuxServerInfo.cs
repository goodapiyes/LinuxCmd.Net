using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Commads;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net
{
    public class LinuxServerInfo
    {
        /// <summary>
        /// 操作系统信息
        /// </summary>
        public string OSName => GetOSName();
        /// <summary>
        /// 累积运行时间
        /// </summary>
        public string RunTime => GetRunTime();
        /// <summary>
        /// 系统负载,任务队列的平均长度
        /// </summary>
        public string LoadAverages => GetLoadAverages();
        /// <summary>
        /// cpu信息
        /// </summary>
        public CpuInfo Cpu => GetCpu();
        /// <summary>
        /// 内存信息
        /// </summary>
        public MemInfo Mem => GetMem();
        /// <summary>
        /// 磁盘信息
        /// </summary>
        public LinuxDfInfo Disk => LinuxHelper.LinuxDisk();
        /// <summary>
        /// 磁盘IO读写信息
        /// </summary>
        public IOInfo IO => GetIO();
        /// <summary>
        /// 网络信息
        /// </summary>
        public LinuxSarInfo NetWork => LinuxHelper.LinuxSar();
        /// <summary>
        /// 进程列表信息
        /// </summary>
        public List<LinuxTopInfo_TaskDetail> Tasks => GetTasks();
        /// <summary>
        /// 网络连接列表信息
        /// </summary>
        public List<LinuxNetstatInfo> NetworkConnections => LinuxHelper.LinuxNetstats();

        private Top top =new Top();
        #region Realize

        protected virtual string GetOSName()
        {
            var os = Utility.GetSingleByRgx("lsb_release -a".LinuxBash().Output, @"Description:\s+(\S+\s+\S+\s+\S+\s+\S+)");
            if (string.IsNullOrEmpty(os))
                return "NaN";
            return os;
        }
        protected virtual string GetRunTime()
        {
            var text = Utility.GetSingleByRgx("top -b -n 1".LinuxBash().Output, @"up\s+(\d+\s+days,\s+\d+:\d+),");
            if (string.IsNullOrEmpty(text))
                return "NaN";
            var days = Utility.GetSingleByRgx(text, @"(\d+)\s+days,");
            var hours = Utility.GetSingleByRgx(text, @",\s+(\d+):");
            var minutes = Utility.GetSingleByRgx(text, @"\d+:(\d+)");
            return $"{days}天 {hours}小时 {minutes}分钟";
        }
        protected virtual string GetLoadAverages()
        {
            var aver = Utility.GetSingleByRgx("top -b -n 1".LinuxBash().Output, @"load\s+average:\s+(.+)\n");
            if (string.IsNullOrEmpty(aver))
                return "NaN";
            else
                return aver.Replace(" ", "");
        }
        protected virtual CpuInfo GetCpu()
        {
            CpuInfo info = new CpuInfo();

            var cores = Utility.GetSingleByRgx("cat /proc/cpuinfo | grep name | cut -f2 -d: | uniq -c".LinuxBash().Output, @"(\d+)\s+");
            if (string.IsNullOrEmpty(cores))
                info.CpuCores = -1;
            else
                info.CpuCores = Convert.ToInt32(cores);

            var cpuName = Utility.GetSingleByRgx("cat /proc/cpuinfo | grep name | cut -f2 -d: | uniq -c".LinuxBash().Output, @"\d+\s+(\S+.+)");
            if (string.IsNullOrEmpty(cpuName))
                info.CpuName = "NaN";
            else
                info.CpuName = cpuName;

            var cpuUsage = Utility.GetSingleByRgx("top -b -n 1".LinuxBash().Output, @"ni,\s*(\S+)\s+id,");
            if (string.IsNullOrEmpty(cpuUsage))
                info.CpuUsage = -1;
            else
            {
                info.CpuUsage = (100 - Convert.ToDouble(cpuUsage)).MathRound();
            }

            return info;
        }
        protected virtual MemInfo GetMem()
        {
            MemInfo info = new MemInfo();

            var total = Utility.GetSingleByRgx("cat /proc/meminfo |grep MemTotal".LinuxBash().Output, @"MemTotal:\s+(\d+)\s+kB");
            if (string.IsNullOrEmpty(total))
                info.MemTotal = -1;
            else
                info.MemTotal = Convert.ToInt32(total) / 1024;

            var available = Utility.GetSingleByRgx("cat /proc/meminfo |grep MemAvailable".LinuxBash().Output, @"MemAvailable:\s+(\d+)\s+kB");
            if (string.IsNullOrEmpty(available))
                info.MemAvailable = -1;
            else
                info.MemAvailable = Convert.ToInt32(available) / 1024;

            var used = Utility.GetSingleByRgx("free -m".LinuxBash().Output, @"Mem:\s+\S+\s+(\S+)\s+");
            if (string.IsNullOrEmpty(used))
                info.MemUsed = -1;
            else
                info.MemUsed = Convert.ToInt32(used);

            var cached = Utility.GetSingleByRgx("cat /proc/meminfo |grep Cached".LinuxBash().Output, @"Cached:\s+(\d+)\s+kB");
            if (string.IsNullOrWhiteSpace(cached))
                info.MemCached = -1;
            else
                info.MemCached = Convert.ToInt32(cached) / 1024;

            var buffers = Utility.GetSingleByRgx("cat /proc/meminfo |grep Buffers".LinuxBash().Output, @"Buffers:\s+(\d+)\s+kB");
            if (string.IsNullOrEmpty(buffers))
                info.MemBuffers = -1;
            else
                info.MemBuffers = Convert.ToInt32(buffers) / 1024;

            info.MemUsage = (100 - Convert.ToDouble((info.MemAvailable * 1.00) / (info.MemTotal * 1.00) * 100)).MathRound();

            return info;
        }
        protected virtual IOInfo GetIO()
        {
            IOInfo info = new IOInfo();
            var result = "sar -b 1 1".LinuxBash().Output;
            if (string.IsNullOrWhiteSpace(result))
                return info;
            Match match = Regex.Match(result, @"Average:\s+\S+\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)");
            if (match.Success)
            {
                info.ReadCount = Convert.ToDouble(match.Groups[1].Value).MathRound();
                info.WriteCount = Convert.ToDouble(match.Groups[2].Value).MathRound();
                info.ReadBytes = Convert.ToDouble(match.Groups[3].Value).MathRound();
                info.WriteBytes = Convert.ToDouble(match.Groups[4].Value).MathRound();
            }
            return info;
        }

        protected virtual List<LinuxTopInfo_TaskDetail> GetTasks()
        {
            return top.GetTaskDetailsByRgx("top -b -n 1".LinuxBash().Output);
        }

        #endregion

    }

    public class CpuInfo
    {
        /// <summary>
        /// cpu描述
        /// </summary>
        public string CpuName { get; set; }
        /// <summary>
        /// cpu核心数
        /// </summary>
        public int CpuCores { get; set; }
        /// <summary>
        /// cpu使用率
        /// </summary>
        public double CpuUsage { get; set; }
    }

    public class MemInfo
    {
        /// <summary>
        /// 内存总容量
        /// </summary>
        public int MemTotal { get; set; }
        /// <summary>
        /// 实际可用内存 MB
        /// </summary>
        public int MemAvailable { get; set; }
        /// <summary>
        /// 已使用的内存 MB
        /// </summary>
        public int MemUsed { get; set; }
        /// <summary>
        /// 缓存化的内存 MB
        /// </summary>
        public int MemCached { get; set; }
        /// <summary>
        /// 系统缓冲 MB
        /// </summary>
        public int MemBuffers { get; set; }
        /// <summary>
        /// 内存相对使用率
        /// </summary>
        public double MemUsage { get; set; }
    }

    public class IOInfo
    {
        public double ReadCount { get; set; }
        public double WriteCount { get; set; }
        public double ReadBytes { get; set; }
        public double WriteBytes { get; set; }
    }
}