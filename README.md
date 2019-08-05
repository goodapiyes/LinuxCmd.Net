# LinuxCmd.Net
.net core linux cmd helper.
1. Running Linux CMD.
2. Get linux server status, include CPU, memory, and networking.

## Example

```C#
//redirect:true Gets the execution result of the server
var lsResult = "ls".LinuxBash().Output;

//redirect:false The result is output to the server
lsResult= "ls".LinuxBash(false).Output;

//Get linux server status, include CPU, memory, and networking. use top cmd
LinuxTopInfo info = LinuxHelper.LinuxTop();

//...
var cpu = info.UserCpu;
var mem = info.MemUsed;
var taskList = info.TaskDetails;
```