namespace LinuxCmd.Net.Models
{
    public class LinuxSarInfo
    {
        /// <summary>
        /// 接收的数据包
        /// </summary>
        public double ReceivedPacket { get; set; }
        /// <summary>
        /// 发送的数据包
        /// </summary>
        public double SentPacket { get; set; }
        /// <summary>
        /// 接收的字节数 kb/s
        /// </summary>
        public double ReceivedBytes { get; set; }
        /// <summary>
        /// 发送的字节数 kb/s
        /// </summary>
        public double SentBytes { get; set; }
    }
}