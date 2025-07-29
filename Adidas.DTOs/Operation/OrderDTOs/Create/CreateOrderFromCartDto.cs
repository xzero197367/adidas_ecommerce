using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class CreateOrderFromCartDto
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string BillingAddress { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? DiscountCode { get; set; }
        public string? Notes { get; set; }
    }

}
