using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Tracker
{
    public class ProductStockDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
        public int VariantCount { get; set; }
        public decimal InventoryValue { get; set; }
    }
}
