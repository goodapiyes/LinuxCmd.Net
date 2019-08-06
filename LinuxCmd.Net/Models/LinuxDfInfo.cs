namespace LinuxCmd.Net.Models
{
    public class LinuxDfInfo
    {
        /// <summary>
        /// 文件系统
        /// </summary>
        public string Filesystem { get; set; }
        /// <summary>
        /// 容量
        /// </summary>
        public double Size { get; set; }
        /// <summary>
        /// 已用
        /// </summary>
        public double Used { get; set; }
        /// <summary>
        /// 可用
        /// </summary>
        public double Avail { get; set; }
        /// <summary>
        /// 已用%
        /// </summary>
        public double UseUsage { get; set; }
        /// <summary>
        /// 挂载点
        /// </summary>
        public string Mounted { get; set; }
    }
}