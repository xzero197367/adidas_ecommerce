using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Calculation
{
    public class OrderItemCalculationDto
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantDetails { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableStock { get; set; }
    }
}
