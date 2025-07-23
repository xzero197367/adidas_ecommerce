using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Operation;

namespace Adidas.Models.Feature;

public class OrderCoupon
{
    [Key]
    public int OrderCouponId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Required]
    public int CouponId { get; set; }

    [ForeignKey("CouponId")]
    public Coupon Coupon { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal DiscountApplied { get; set; }
}