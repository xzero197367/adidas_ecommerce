using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Operation;
using Models.Feature;

namespace Adidas.Models.Feature;

public class OrderCoupon : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Required]
    public Guid CouponId { get; set; }

    [ForeignKey("CouponId")]
    public Discount Coupon { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal DiscountApplied { get; set; }
}