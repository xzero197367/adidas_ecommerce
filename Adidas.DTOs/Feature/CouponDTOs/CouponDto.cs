using Adidas.DTOs.Common_DTOs;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponDto : BaseDto
    {
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required DiscountType DiscountType { get; set; }
        public required decimal DiscountValue { get; set; }
        public decimal MinimumAmount { get; set; }
        public required DateTime ValidFrom { get; set; }
        public required DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        
        public decimal DiscountAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}