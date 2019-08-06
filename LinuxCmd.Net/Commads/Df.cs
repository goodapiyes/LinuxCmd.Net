using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.Commads
{
    public class Df
    {
        public List<LinuxDfInfo> GetDfInfos(string input)
        {
            var matchs = Regex.Matches(input, @"\s*(\S+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\S+)%\s+(\S+)");
            List<LinuxDfInfo> list = new List<LinuxDfInfo>();
            foreach (Match match in matchs)
            {
                if (match.Success)
                {
                    LinuxDfInfo info = new LinuxDfInfo();
                    info.Filesystem = match.Groups[1].Value;
                    info.Size = Convert.ToDouble(match.Groups[2].Value) /1024;
                    info.Used = Convert.ToDouble(match.Groups[3].Value) / 1024;
                    info.Avail = Convert.ToDouble(match.Groups[4].Value) / 1024;
                    info.UseUsage = Convert.ToDouble(match.Groups[5].Value);
                    info.Mounted = match.Groups[6].Value;
                    list.Add(info);
                }
            }
            return list;
        }

        public LinuxDfInfo GetDiskInfo(string input)
        {
            return GetDfInfos(input)[0];
        }
    }
}