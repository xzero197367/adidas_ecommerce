

namespace Adidas.DTOs.Static
{
    public class CustomerSegmentDto
    {
        public string SegmentName { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
