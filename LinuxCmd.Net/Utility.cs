using System;
using System.Text.RegularExpressions;

namespace LinuxCmd.Net
{
    public static class Utility
    {
        public static string GetSingleByRgx(string input, string pattern)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : "";
        }

        public static double MathRound(this double d)
        {
            return Math.Round(d, 1);
        }
    }
}