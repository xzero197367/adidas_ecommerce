using Adidas.DTOs.CommonDTOs;
using Adidas.Models.Operation;

namespace Adidas.DTOs.Operation.OrderDTOs;

public class OrderUpdateDto : BaseUpdateDto
{

    public string? OrderNumber { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }
    public string? ShippingAddress { get; set; } // JSON string?
    public string? BillingAddress { get; set; } // JSON string?

    public string? Notes { get; set; }

    // foreign keys
    public required String UserId { get; set; }
}