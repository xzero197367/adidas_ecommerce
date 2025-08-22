using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class CreateOrderFromCartDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ShippingAddress { get; set; } // JSON string

        [Required]
        public string BillingAddress { get; set; } // JSON string

        [Required]
        public string Currency { get; set; } = "USD";

        public string? CouponCode { get; set; }

        public string? Notes { get; set; }

        public string? PaymentMethodId { get; set; }
    }
}