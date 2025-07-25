using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Feature;
using Models.People;

namespace Adidas.Models.Operation;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class Order : BaseAuditableEntity
{
    // fields
    public required string OrderNumber { get; set; }
    public required OrderStatus OrderStatus { get; set; }
    public required decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }

    public required decimal TotalAmount { get; set; }
    public required string Currency { get; set; }
    public required DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }
    public required string ShippingAddress { get; set; } // JSON string
    public required string BillingAddress { get; set; } // JSON string

    public string Notes { get; set; }
    
    // foreign keys
    public required Guid UserId { get; set; }

    // navigation properties
    public User User { get; set; }
    
    // Relationships
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
}