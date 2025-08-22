using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class CreateOrderDTO
    {
        public string UserId { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? Currency { get; set; } // JSON string
        

    }
}
