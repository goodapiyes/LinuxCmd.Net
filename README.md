# LinuxCmd.Net
.net core linux cmd helper.
1. Running Linux CMD.
2. Get linux server status, include CPU, memory, and networking.

## Example

```C#
//redirect:true Gets the execution result of the server
var ls = "ls".LinuxBash().Output;

//redirect:false The result is output to the server
ls = "ls".LinuxBash(false).Output;

//Get linux server status, include CPU, memory, and networking. use top cmd
LinuxTopInfo top = LinuxHelper.LinuxTop();

//The result is output to the server
$"echo {top.TaskDetails[0].SerializeJSON()}".LinuxBash(false);

//cpu usage
var cpu = top.UserCpu;

//mem usage
var mem = top.MemUsed;

//server load
var averages = top.Averages;

//The process list
var taskList = top.TaskDetails;

//Disk status
LinuxDfInfo disk = LinuxHelper.LinuxDisk();
$"echo {disk.SerializeJSON()}".LinuxBash(false);

//Disk read/write rate
LinuxVmstatInfo vmstat = LinuxHelper.LinuxVmstat();
$"echo {vmstat.SerializeJSON()}".LinuxBash(false);

//Network packets
LinuxSarInfo sar = LinuxHelper.LinuxSar();
$"echo {sar.SerializeJSON()}".LinuxBash(false);

//Tcp Network Connections
var netstats = LinuxHelper.LinuxNetstats();
foreach (var net in netstats)
{
    $"echo {net.SerializeJSON()}".LinuxBash(false);
}
```