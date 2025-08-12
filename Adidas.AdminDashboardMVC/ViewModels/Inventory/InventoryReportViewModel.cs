using Adidas.DTOs.Tracker;

namespace Adidas.AdminDashboardMVC.ViewModels.Inventory
{
    public class InventoryReportViewModel
    {
        public InventoryReportDto Report { get; set; } = new InventoryReportDto
        {
            TotalProducts = 0,
            TotalVariants = 0,
            LowStockVariants = 0,
            OutOfStockVariants = 0,
            TotalInventoryValue = 0,
            ProductStocks = new List<ProductStockDto>()
        };

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
