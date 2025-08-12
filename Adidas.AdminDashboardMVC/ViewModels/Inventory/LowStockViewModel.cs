using Adidas.DTOs.Tracker;
using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Inventory
{
    public class LowStockViewModel
    {
        public IEnumerable<LowStockAlertDto> Alerts { get; set; } = new List<LowStockAlertDto>();

        [Display(Name = "Threshold")]
        [Range(1, 100, ErrorMessage = "Threshold must be between 1 and 100")]
        public int Threshold { get; set; } = 10;

    }
}
