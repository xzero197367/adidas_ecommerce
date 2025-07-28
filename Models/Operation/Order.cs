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

public class Order : BaseEntity
{
    [Required, MaxLength(50)]
    public string OrderNumber { get; set; }

    [Required]
    public String UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public OrderStatus OrderStatus { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    [Required]
    public string ShippingAddress { get; set; } // JSON string

    [Required]
    public string BillingAddress { get; set; } // JSON string

    public string Notes { get; set; }

    // Relationships
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
}