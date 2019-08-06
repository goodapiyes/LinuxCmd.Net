using System.Diagnostics;
using System.Runtime.InteropServices;
using LinuxCmd.Net.Commads;
using LinuxCmd.Net.Models;

namespace LinuxCmd.Net
{
    public static class LinuxHelper
    {
        public static BashResult LinuxBash(this string cmd,bool redirect = true)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new BashResult() { Error = "The current system does not support it!", Success = false, Output = null };
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
            var result = "top -b -n 1".LinuxBash();
            return result.Success ? (new Top()).GetTop(result.Output) : null;
        }

        public static LinuxDfInfo LinuxDisk()
        {
            var result = "df".LinuxBash();
            return result.Success ? (new Df()).GetDiskInfo(result.Output) : null;
        }

        public static LinuxVmstatInfo LinuxVmstat()
        {
            var result = "vmstat".LinuxBash();
            return result.Success ? (new Vmstat()).GetLinuxVmstatInfo(result.Output) : null;
        }
    }
}