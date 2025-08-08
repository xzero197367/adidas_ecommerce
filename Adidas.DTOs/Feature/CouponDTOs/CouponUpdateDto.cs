using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs;

public class CouponUpdateDto:BaseUpdateDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? UsageLimit { get; set; }
}