# LinuxCmd.Net
.net core linux cmd helper.
1. Running Linux CMD.
2. Get linux server status, include CPU, memory, and networking.

## Example

```C#
//redirect:true ��ȡ����������ִ�н��
var ls = "ls".LinuxBash().Output;

//redirect:false ������ִ�н���ض��������������
ls = "ls".LinuxBash(false).Output;

//������״̬����
LinuxServerInfo server = new LinuxServerInfo();

//ϵͳ��Ϣ
var OSName = server.OSName;
$"echo {OSName}".LinuxBash(false);

//����ʱ��
var RunTime = server.RunTime;
$"echo {RunTime}".LinuxBash(false);

//ϵͳ����
var LoadAverages = server.LoadAverages;
$"echo {LoadAverages}".LinuxBash(false);

//CPU״̬: cpu����,cpu������,cpuʹ����
var cpuInfo = server.Cpu;
$"echo {cpuInfo.SerializeJSON().Replace('(',' ').Replace(')',' ')}".LinuxBash(false);

//�ڴ�״̬:�ڴ�������,ʵ�ʿ�������,��ʹ�õ�����,���滯������,ϵͳ��������
var Mem = server.Mem;
$"echo {Mem.SerializeJSON()}".LinuxBash(false);

//����״̬:����������,��������,��������,���ðٷֱ�
var Disk = server.Disk;
$"echo {Disk.SerializeJSON()}".LinuxBash(false);

//IO��д״̬:����������,д��������,���ֽ���,д�ֽ���
var IO = server.IO;
$"echo {IO.SerializeJSON()}".LinuxBash(false);

//����״̬:���յ����ݰ�����,���͵����ݰ�����,�����ֽ���,�����ֽ���
var NetWork = server.NetWork;
$"echo {NetWork.SerializeJSON()}".LinuxBash(false);

//��������״̬: tcp�ͻ���IP,������IP,����״̬
var NetworkConnections = server.NetworkConnections;
foreach (var net in NetworkConnections)
{
    $"echo {net.SerializeJSON()}".LinuxBash(false);
}

//�����б�:����id,���������ߵ��û���,�����ڴ�ʹ����,�����ڴ�ʹ����,����״̬,CPUʹ����,����������
var Tasks = server.Tasks;
for (int i = 0; i < 6; i++)
{
    $"echo {Tasks[i].SerializeJSON()}".LinuxBash(false);
}
```