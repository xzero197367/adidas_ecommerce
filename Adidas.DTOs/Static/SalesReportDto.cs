using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Static
{
    public class SalesReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public IEnumerable<DailySalesDto> DailySales { get; set; } = new List<DailySalesDto>();
        public IEnumerable<CategorySalesDto> CategorySales { get; set; } = new List<CategorySalesDto>();
    }
}
