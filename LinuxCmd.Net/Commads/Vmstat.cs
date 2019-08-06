using System;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.Commads
{
    public class Vmstat
    {
        public virtual LinuxVmstatInfo GetLinuxVmstatInfo(string input)
        {
            Match match = Regex.Match(input, @"(\d+)\s+(\d+)\s+\d+\s+\d+\s+\d+\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)");
            if (match.Success)
            {
                LinuxVmstatInfo info = new LinuxVmstatInfo();
                info.run = Convert.ToInt32(match.Groups[1].Value);
                info.block = Convert.ToInt32(match.Groups[2].Value);
                info.bi = Convert.ToInt32(match.Groups[3].Value);
                info.bo = Convert.ToInt32(match.Groups[4].Value);
                return info;
            }
            return null;
        }
    }
}