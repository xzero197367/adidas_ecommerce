using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Calculation
{
    public class OrderCalculationDto
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemCalculationDto> Items { get; set; } = new();
        public List<AppliedCouponDto> AppliedCoupons { get; set; } = new();
    }
}
