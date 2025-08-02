using Adidas.DTOs.Common_DTOs;

namespace Adidas.DTOs.Feature.OrderCouponDTOs;

public class OrderCouponDto: BaseDto
{
    // fields
    public decimal DiscountApplied { get; set; }
    
    // foreign key
    public Guid CouponId { get; set; }
    public Guid OrderId { get; set; }
}