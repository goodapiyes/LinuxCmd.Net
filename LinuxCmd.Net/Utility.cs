using System.Text.RegularExpressions;

namespace LinuxCmd.Net
{
    public class Utility
    {
        public static string GetSingleByRgx(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : "";
        }
    }
}