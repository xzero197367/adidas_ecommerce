
namespace Adidas.DTOs.Static
{
    public class CategoryPerformanceDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
    }
}
