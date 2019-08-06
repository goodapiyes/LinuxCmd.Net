using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net.Commads
{
    public class Netstat
    {
        public virtual List<LinuxNetstatInfo> GetLinuxNetstatInfos(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"(tcp)\s+\d+\s+\d+\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)");
            List<LinuxNetstatInfo> list = new List<LinuxNetstatInfo>();
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    LinuxNetstatInfo info = new LinuxNetstatInfo();
                    info.Proto = match.Groups[1].Value;
                    info.LocalAddress = match.Groups[2].Value;
                    info.ForeignAddress = match.Groups[3].Value;
                    info.State = match.Groups[4].Value;
                    info.ProgramName = match.Groups[5].Value;
                    list.Add(info);
                }
            }
            return list;
        }
    }
}