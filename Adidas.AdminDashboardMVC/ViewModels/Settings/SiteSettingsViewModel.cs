using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Settings
{
    public class SiteSettingsViewModel
    {
        [Required]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "";

        [Display(Name = "Site Description")]
        public string SiteDescription { get; set; } = "";

        [Required]
        [Url]
        [Display(Name = "Site URL")]
        public string SiteUrl { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; } = "";

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; }

        [Display(Name = "Allow User Registration")]
        public bool AllowRegistration { get; set; }

        [Display(Name = "Email Verification Required")]
        public bool EmailVerificationRequired { get; set; }

        [Range(1, 10)]
        [Display(Name = "Max Login Attempts")]
        public int MaxLoginAttempts { get; set; }

        [Range(5, 120)]
        [Display(Name = "Session Timeout (minutes)")]
        public int SessionTimeout { get; set; }

        [Range(5, 100)]
        [Display(Name = "Default Page Size")]
        public int PageSize { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; } = "";

        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "";

        [Display(Name = "Currency")]
        public string Currency { get; set; } = "";
    }
}
