
using System.ComponentModel.DataAnnotations;
using Adidas.Models.Operation;
using System.Collections.Generic;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class OrderCreateDto
    {
        public required string OrderNumber { get; set; }
        public required OrderStatus OrderStatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public required decimal TotalAmount { get; set; }
        public required string Currency { get; set; } = "USD";
        public required DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        
        [Required]
        public required string ShippingAddress { get; set; } // JSON string
        
        [Required]
        public required string BillingAddress { get; set; } // JSON string

        public string Notes { get; set; } = string.Empty;
    
        // Foreign keys
        [Required]
        public required string UserId { get; set; }

        // Collection of order items
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<OrderItemCreateDto> OrderItems { get; set; } = new();
    }
}
