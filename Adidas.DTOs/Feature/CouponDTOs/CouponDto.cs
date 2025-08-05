using Adidas.DTOs.Common_DTOs;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs;

public class CouponDto: BaseDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinimumAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    
    // calculated properties
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal OriginalAmount { get; set; }
}