using System.ComponentModel.DataAnnotations;
using Adidas.Models.Operation;
using System.Collections.Generic;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class OrderCreateDto
    {
        [Required]
        public string OrderNumber { get; set; }
        public required OrderStatus OrderStatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        [Required] public decimal TotalAmount { get; set; }
        [Required] public string Currency { get; set; } = "USD";
        [Required] public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        [Required] public string ShippingAddress { get; set; } // JSON string

        [Required] public string BillingAddress { get; set; } // JSON string

        public string Notes { get; set; } = string.Empty;

        // Foreign keys
        [Required] public string UserId { get; set; }

        // Collection of order items
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<OrderItemCreateDto> OrderItems { get; set; } = new();
    }
}