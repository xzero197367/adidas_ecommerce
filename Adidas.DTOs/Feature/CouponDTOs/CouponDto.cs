using Adidas.DTOs.Common_DTOs;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }

        // Computed properties for convenience
        public bool IsExpired => ValidTo < DateTime.UtcNow;
        public bool IsValidNow => DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo && IsActive && !IsDeleted;
        public bool HasUsageLimit => UsageLimit > 0;
        public bool IsUsageLimitReached => HasUsageLimit && UsedCount >= UsageLimit;
        public string DiscountDisplayText => DiscountType == DiscountType.Percentage
            ? $"{DiscountValue}%"
            : $"${DiscountValue:F2}";
        public string StatusText => IsExpired ? "Expired" :
                                   IsUsageLimitReached ? "Limit Reached" :
                                   IsValidNow ? "Active" : "Inactive";
        public int RemainingUsage => HasUsageLimit ? Math.Max(0, UsageLimit - UsedCount) : int.MaxValue;
        public decimal UsagePercentage => HasUsageLimit && UsageLimit > 0
            ? Math.Min(100, (decimal)UsedCount / UsageLimit * 100)
            : 0;

        public bool IsDeleted { get; private set; }
    }
}