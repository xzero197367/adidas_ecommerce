using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Tracker
{
    public class LowStockAlertDto
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantDetails { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
    }
}
