using Adidas.DTOs.Common_DTOs;
using Models.Feature;
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponCreateDto : IValidatableObject
    {
        [Required(ErrorMessage = "Coupon code is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 50 characters")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must contain only uppercase letters and numbers")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Coupon name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum amount cannot be negative")]
        public decimal MinimumAmount { get; set; } = 0;

        [Required(ErrorMessage = "Valid from date is required")]
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Valid to date is required")]
        public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddMonths(1);

        [Range(0, int.MaxValue, ErrorMessage = "Usage limit cannot be negative")]
        public int UsageLimit { get; set; } = 0; // 0 means unlimited

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Custom validation method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validate date range
            if (ValidTo <= ValidFrom)
            {
                results.Add(new ValidationResult(
                    "Valid to date must be after valid from date",
                    new[] { nameof(ValidTo) }));
            }

            // Validate ValidFrom is not in the past (allow some tolerance for timezone differences)
            if (ValidFrom < DateTime.UtcNow.AddHours(-1))
            {
                results.Add(new ValidationResult(
                    "Valid from date cannot be in the past",
                    new[] { nameof(ValidFrom) }));
            }

            // Validate percentage discount
            if (DiscountType == DiscountType.Percentage)
            {
                if (DiscountValue > 100)
                {
                    results.Add(new ValidationResult(
                        "Percentage discount cannot exceed 100%",
                        new[] { nameof(DiscountValue) }));
                }

                if (DiscountValue < 0.01m)
                {
                    results.Add(new ValidationResult(
                        "Percentage discount must be at least 0.01%",
                        new[] { nameof(DiscountValue) }));
                }
            }

            // Validate fixed amount discount
            if (DiscountType == DiscountType.FixedAmount)
            {
                if (DiscountValue > 10000) // Reasonable upper limit
                {
                    results.Add(new ValidationResult(
                        "Fixed discount amount seems too high. Please verify the amount.",
                        new[] { nameof(DiscountValue) }));
                }
            }

            // Validate minimum amount logic
            if (DiscountType == DiscountType.FixedAmount && MinimumAmount > 0 && DiscountValue >= MinimumAmount)
            {
                results.Add(new ValidationResult(
                    "Fixed discount amount should be less than minimum order amount",
                    new[] { nameof(DiscountValue) }));
            }

            return results;
        }
    }
}