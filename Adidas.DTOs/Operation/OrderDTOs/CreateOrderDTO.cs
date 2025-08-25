using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Feature.ShoppingCartDTOS;

namespace Adidas.DTOs.Operation.OrderDTOs
{
    // CreateOrderDTO - Add these properties
    public class CreateOrderDTO
    {
        public required string UserId { get; set; } // For guests: "guest_[guid]"
        public bool IsGuestUser { get; set; }
        public string? GuestEmail { get; set; } // Required if IsGuestUser is true

        public required string ShippingAddress { get; set; }
        public required string BillingAddress { get; set; }
        public required string Currency { get; set; }
        public string? CouponCode { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }

        // Cart items - required for both authenticated and guest users
        public required List<ShoppingCartDto> CartItems { get; set; }
    }

}
