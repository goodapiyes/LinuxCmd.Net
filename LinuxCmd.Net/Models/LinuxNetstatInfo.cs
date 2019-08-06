namespace LinuxCmd.Net.Models
{
    public class LinuxNetstatInfo
    {
        public string Proto { get; set; }
        public string LocalAddress { get; set; }
        public string ForeignAddress { get; set; }
        public string State { get; set; }
        public string ProgramName { get; set; }
    }
}