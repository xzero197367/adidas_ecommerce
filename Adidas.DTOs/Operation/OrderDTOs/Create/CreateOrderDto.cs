using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class CreateOrderDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Currency { get; set; } = "EGP";

        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }

        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();

        public List<string> CouponCodes { get; set; } = new();

    }

}
