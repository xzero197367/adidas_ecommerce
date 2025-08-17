using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class BillingSummaryDto
    {
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public string ShippingText { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }
}
