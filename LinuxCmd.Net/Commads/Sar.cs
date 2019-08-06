using System;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.Commads
{
    public class Sar
    {
        public LinuxSarInfo GetLinuxSarInfo(string input)
        {
            Match match = Regex.Match(input, @"Average:\s+eth0\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)");
            if (match.Success)
            {
                LinuxSarInfo info = new LinuxSarInfo();
                info.ReceivedPacket = Convert.ToDouble(match.Groups[1].Value);
                info.SentPacket = Convert.ToDouble(match.Groups[2].Value);
                info.ReceivedBytes = Convert.ToDouble(match.Groups[3].Value);
                info.SentBytes = Convert.ToDouble(match.Groups[4].Value);
                return info;
            }
            return null;
        }
    }
}