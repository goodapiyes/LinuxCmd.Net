using System.Collections.Generic;

namespace LinuxCmd.Net.Models
{
    public class LinuxTopInfo
    {
        #region 运行时间,运行状态,负载
        /// <summary>
        /// 统计时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 服务器运行天数
        /// </summary>
        public int Days { get; set; }
        /// <summary>
        /// 当前服务器SSH登录人数
        /// </summary>
        public int Users { get; set; }
        /// <summary>
        /// 当前服务器负载
        /// </summary>
        public string Averages { get; set; }
        #endregion

        #region 进程统计
        /// <summary>
        /// 进程总数
        /// </summary>
        public int TaskTotal { get; set; }
        /// <summary>
        /// 正在运行进程数
        /// </summary>
        public int RunningCount { get; set; }
        /// <summary>
        /// 睡眠进程数
        /// </summary>
        public int SleepingCount { get; set; }
        /// <summary>
        /// 停止进程数
        /// </summary>
        public int StoppedCount { get; set; }
        /// <summary>
        /// 僵尸进程数
        /// </summary>
        public int ZombieCount { get; set; }
        #endregion

        #region cpu统计
        /// <summary>
        /// 用户空间占用CPU百分比
        /// </summary>
        public double UserCpu { get; set; }
        /// <summary>
        /// 内核空间占用CPU百分比
        /// </summary>
        public double SystemCpu { get; set; }
        /// <summary>
        /// 用户进程空间内改变过优先级的进程占用CPU百分比
        /// </summary>
        public double NiCpu { get; set; }
        /// <summary>
        /// 空闲CPU百分比
        /// </summary>
        public double FreeCpu { get; set; }
        /// <summary>
        /// 等待输入输出的CPU时间百分比
        /// </summary>
        public double WaitCpu { get; set; }
        #endregion

        #region 物理内存统计
        /// <summary>
        /// 物理内存总量
        /// </summary>
        public double MemTotal { get; set; }
        /// <summary>
        /// 空闲内存总量
        /// </summary>
        public double MemFree { get; set; }
        /// <summary>
        /// 使用的物理内存总量
        /// </summary>
        public double MemUsed { get; set; }
        /// <summary>
        /// 用作内核缓存的内存量
        /// </summary>
        public double MemCache { get; set; }
        #endregion

        #region 虚拟内存统计
        /// <summary>
        /// 交换分区总量
        /// </summary>
        public double SwapTotal { get; set; }
        /// <summary>
        /// 空闲交换区总量
        /// </summary>
        public double SwapFree { get; set; }
        /// <summary>
        /// 使用的交换区总量
        /// </summary>
        public double SwapUsed { get; set; }
        /// <summary>
        /// 缓冲的交换区总量。
        /// </summary>
        public double SwapCache { get; set; }
        #endregion
        /// <summary>
        /// 进程信息列表
        /// </summary>
        public List<LinuxTopInfo_TaskDetail> TaskDetails { get; set; }
    }

    public class LinuxTopInfo_TaskDetail
    {
        /// <summary>
        /// 进程id
        /// </summary>
        public string Pid { get; set; }
        /// <summary>
        /// 进程所有者的用户名
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 进程使用的虚拟内存总量 
        /// </summary>
        public double Virt { get; set; }
        /// <summary>
        /// 进程使用的、未被换出的物理内存大小
        /// </summary>
        public double Res { get; set; }
        /// <summary>
        /// 共享内存大小
        /// </summary>
        public double Shr { get; set; }
        /// <summary>
        ///进程状态。
        ///D=    不可中断的睡眠状态
        ///R=    运行
        ///S=    睡眠
        ///T=    跟踪/停止
        ///Z=    僵尸进程
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 上次更新到现在的CPU时间占用百分比
        /// </summary>
        public double Cpu { get; set; }
        /// <summary>
        /// 进程使用的物理内存百分比
        /// </summary>
        public double Mem { get; set; }
        /// <summary>
        /// 进程使用的CPU时间总计
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 命令名/命令行
        /// </summary>
        public string Commad { get; set; }
    }
}
