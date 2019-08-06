namespace LinuxCmd.Net.Models
{
    public class LinuxVmstatInfo
    {
        /// <summary>
        /// 处于运行的状态的进程数
        /// </summary>
        public int run { get; set; }
        /// <summary>
        /// 进程被cpu以外的状态给阻断了，
        /// 比如是硬盘，网络，当我们进程发一个数据包，网速快很快就能发完
        /// 但是当网速太慢，就会导致b的状态
        /// </summary>
        public int block { get; set; }
        /// <summary>
        /// 磁盘读取总量 kb/s
        /// </summary>
        public int bi { get; set; }
        /// <summary>
        /// 磁盘写入总量 kb/s
        /// </summary>
        public int bo { get; set; }
    }
}