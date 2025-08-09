
using Adidas.Models.Operation;
using Coupon = Models.Feature.Coupon;

namespace Adidas.Models.Feature;

public class OrderCoupon : BaseAuditableEntity
{
    // fields
    public decimal DiscountApplied { get; set; }
    
    // foreign key
    public Guid CouponId { get; set; }
    public Guid OrderId { get; set; }

    // navigation
    public Coupon Coupon { get; set; }
    public Order Order { get; set; }
  
}