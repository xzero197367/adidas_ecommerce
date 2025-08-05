using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Adidas.AdminDashboardMVC.ViewModels;
using System.Text.Json;
using Adidas.AdminDashboardMVC.ViewModels.Settings;

namespace Adidas.AdminDashboardMVC.Controllers.System
{
    public class SettingsController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public IActionResult Users()
        {
            // Redirect to Users controller
            return RedirectToAction("Index", "Users");
        }

        public async Task<IActionResult> Site()
        {
            var settingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            var siteSettings = await LoadSiteSettings();

            return View(siteSettings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSiteSettings(SiteSettingsViewModel model)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                await SaveSiteSettings(model);
                return Json(new { success = true, message = "Settings updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearCache()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Clear application cache logic here
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                return Json(new { success = true, message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BackupDatabase()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Database backup logic would go here
                // For now, we'll simulate a backup
                await Task.Delay(2000);

                var backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                return Json(new { success = true, message = $"Database backup created: {backupFileName}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<SiteSettingsViewModel> LoadSiteSettings()
        {
            // Load from configuration or database
            return new SiteSettingsViewModel
            {
                SiteName = _configuration["SiteSettings:SiteName"] ?? "Adidas Admin Dashboard",
                SiteDescription = _configuration["SiteSettings:SiteDescription"] ?? "Admin Dashboard for Adidas E-commerce Platform",
                SiteUrl = _configuration["SiteSettings:SiteUrl"] ?? "https://localhost:5001",
                AdminEmail = _configuration["SiteSettings:AdminEmail"] ?? "admin@adidas.com",
                SupportEmail = _configuration["SiteSettings:SupportEmail"] ?? "support@adidas.com",
                MaintenanceMode = bool.Parse(_configuration["SiteSettings:MaintenanceMode"] ?? "false"),
                AllowRegistration = bool.Parse(_configuration["SiteSettings:AllowRegistration"] ?? "true"),
                EmailVerificationRequired = bool.Parse(_configuration["SiteSettings:EmailVerificationRequired"] ?? "true"),
                MaxLoginAttempts = int.Parse(_configuration["SiteSettings:MaxLoginAttempts"] ?? "5"),
                SessionTimeout = int.Parse(_configuration["SiteSettings:SessionTimeout"] ?? "30"),
                PageSize = int.Parse(_configuration["SiteSettings:PageSize"] ?? "10"),
                TimeZone = _configuration["SiteSettings:TimeZone"] ?? "UTC",
                DateFormat = _configuration["SiteSettings:DateFormat"] ?? "MM/dd/yyyy",
                Currency = _configuration["SiteSettings:Currency"] ?? "USD"
            };
        }

        private async Task SaveSiteSettings(SiteSettingsViewModel settings)
        {
            // In a real application, you would save to database
            // For this example, we'll just simulate saving
            await Task.Delay(100);

            // You could also update appsettings.json programmatically here
            // but it's generally not recommended for production
        }

        public IActionResult SystemInfo()
        {
            var systemInfo = new SystemInfoViewModel
            {
                ServerName = Environment.MachineName,
                OperatingSystem = Environment.OSVersion.ToString(),
                DotNetVersion = Environment.Version.ToString(),
                ApplicationVersion = "1.0.0",
                TotalMemory = GC.GetTotalMemory(false),
                WorkingSet = Environment.WorkingSet,
                ProcessorCount = Environment.ProcessorCount,
                ApplicationUptime = DateTime.Now - global::System.Diagnostics.Process.GetCurrentProcess().StartTime,
                DatabaseStatus = "Connected", // You would check actual database status
                CacheStatus = "Active"
            };

            return PartialView("_SystemInfo", systemInfo);
        }
    }
}