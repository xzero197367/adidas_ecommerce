

namespace Adidas.DTOs.Tracker
{
    public class InventoryReportDto
    {
        public int TotalProducts { get; set; }
        public int TotalVariants { get; set; }
        public int LowStockVariants { get; set; }
        public int OutOfStockVariants { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public IEnumerable<ProductStockDto> ProductStocks { get; set; } = new List<ProductStockDto>();
    }
}
