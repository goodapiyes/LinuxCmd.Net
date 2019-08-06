using System;
using System.IO;

namespace LinuxCmd.Net
{
    public class LinuxServerInfo
    {
        public string OSName => GetOSName();

        public string RunTime => GetRunTime();

        public string CpuCores=> GetCpuCores();

        public string CpuName => GetCpuName();

        public double CpuUsage => GetCpuUsage();

        public int MemTotal => GetMemTotal();

        public int MemAvailable => GetMemAvailable();

        public int MemUsed => GetMemUsed();
        protected virtual string GetOSName()
        {
            return Utility.GetSingleByRgx("lsb_release -a".LinuxBash().Output, @"Description:\s+(\S+\s+\S+\s+\S+\s+\S+)");
        }

        protected virtual string GetRunTime()
        {
            var text = Utility.GetSingleByRgx("top -b -n 1".LinuxBash().Output, @"up\s+(\d+\s+days,\s+\d+:\d+),");
            var days = Utility.GetSingleByRgx(text, @"(\d+)\s+days,");
            var hours= Utility.GetSingleByRgx(text, @",\s+(\d+):");
            var minutes = Utility.GetSingleByRgx(text, @"\d+:(\d+)");
            return $"{days}天 {hours}小时 {minutes}分钟";
        }

        protected virtual string GetCpuCores()
        {
            return Utility.GetSingleByRgx("cat /proc/cpuinfo | grep name | cut -f2 -d: | uniq -c".LinuxBash().Output, @"(\d+)\s+");
        }

        protected virtual string GetCpuName()
        {
            return Utility.GetSingleByRgx("cat /proc/cpuinfo | grep name | cut -f2 -d: | uniq -c".LinuxBash().Output, @"\d+\s+(\S+.+)");
        }

        protected virtual double GetCpuUsage()
        {
            var input = "top -b -n 1".LinuxBash().Output;
            double free = Convert.ToDouble(Utility.GetSingleByRgx(input, @"ni,\s*(\S+)\s+id,"));
            return 100 - free;
        }

        protected virtual int GetMemTotal()
        {
            double total = Convert.ToDouble(Utility.GetSingleByRgx("cat /proc/meminfo |grep MemTotal".LinuxBash().Output, @"MemTotal:\s+(\d+)\s+kB")) / 1024;
            return Convert.ToInt32(total);
        }

        protected virtual int GetMemAvailable()
        {
            double total = Convert.ToDouble(Utility.GetSingleByRgx("cat /proc/meminfo |grep MemAvailable".LinuxBash().Output, @"MemAvailable:\s+(\d+)\s+kB")) / 1024;
            return Convert.ToInt32(total);
        }

        protected virtual int GetMemUsed()
        {
            double total = Convert.ToDouble(Utility.GetSingleByRgx("free -m".LinuxBash().Output, @"Mem:\s+\S+\s+(\S+)\s+"));
            return Convert.ToInt32(total);
        }

    }
}