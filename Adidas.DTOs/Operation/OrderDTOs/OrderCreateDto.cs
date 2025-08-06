
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class OrderCreateDto
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
