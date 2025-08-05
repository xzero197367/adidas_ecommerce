using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Result;

namespace Adidas.DTOs.Feature.OrderCouponDTOs;

public class OrderCouponDto: BaseDto
{
    // fields
    public decimal DiscountApplied { get; set; }
    
    // foreign key
    public Guid CouponId { get; set; }
    public Guid OrderId { get; set; }
    
    // navigation
    public CouponDto? Coupon { get; set; }
    public OrderDto? Order { get; set; }
}