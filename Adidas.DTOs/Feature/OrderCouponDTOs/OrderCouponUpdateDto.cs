using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Feature.OrderCouponDTOs;

public class OrderCouponUpdateDto: BaseUpdateDto
{
    // fields
    public decimal? DiscountApplied { get; set; }
}