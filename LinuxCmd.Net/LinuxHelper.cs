using System.Diagnostics;
using LinuxCmd.Net.Commads;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net
{
    public static class LinuxHelper
    {
        public static BashResult LinuxBash(this string cmd,bool redirect = true)
        {
            var args = cmd.Replace("\"", "\\\"");

            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{args}\"",
                RedirectStandardOutput = redirect,
                RedirectStandardError = redirect,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };

            using (var process = new Process() {StartInfo = startInfo})
            {
                process.Start();
                string result = redirect ? process.StandardOutput.ReadToEnd() : "";
                string error = redirect ? process.StandardError.ReadToEnd() : "";
                process.WaitForExit();
                int code = process.ExitCode;
                process.Close();
                return new BashResult() {Success = (code == 0), Error = error, Output = result};
            }
        }

        public static LinuxTopInfo LinuxTop()
        {
            return (new Top()).GetTop("top -b -n 1".LinuxBash().Output);
        }
    }
}