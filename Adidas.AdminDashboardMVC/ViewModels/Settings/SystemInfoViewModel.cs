namespace Adidas.AdminDashboardMVC.ViewModels.Settings
{
    public class SystemInfoViewModel
    {
        public string ServerName { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string DotNetVersion { get; set; } = "";
        public string ApplicationVersion { get; set; } = "";
        public long TotalMemory { get; set; }
        public long WorkingSet { get; set; }
        public int ProcessorCount { get; set; }
        public TimeSpan ApplicationUptime { get; set; }
        public string DatabaseStatus { get; set; } = "";
        public string CacheStatus { get; set; } = "";
    }
}
