using System;
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.Common_DTOs;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponUpdateDto : IValidatableObject
    {
        public Guid Id { get; set; }
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ValidTo <= ValidFrom)
            {
                yield return new ValidationResult(
                    "Valid To date must be after Valid From date.",
                    new[] { nameof(ValidTo) }
                );
            }

            // Optional: Ensure ValidFrom is not in the past
            if (ValidFrom < DateTime.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "Valid From date cannot be in the past.",
                    new[] { nameof(ValidFrom) }
                );
            }
            
        }
    }
}

 
