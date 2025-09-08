using Models.People;
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.ProductDTOs
{
    public class ProductFilterDto : IValidatableObject
    {
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public Gender? Gender { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsOnSale { get; set; }
        public bool? InStock { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;

        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }

        // Validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // MinPrice <= MaxPrice
            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
            {
                yield return new ValidationResult(
                    "MinPrice cannot be greater than MaxPrice.",
                    new[] { nameof(MinPrice), nameof(MaxPrice) });
            }

            // MinStock <= MaxStock
            if (MinStock.HasValue && MaxStock.HasValue && MinStock.Value > MaxStock.Value)
            {
                yield return new ValidationResult(
                    "MinStock cannot be greater than MaxStock.",
                    new[] { nameof(MinStock), nameof(MaxStock) });
            }

            // Optional: CreatedAfter <= CreatedBefore
            if (CreatedAfter.HasValue && CreatedBefore.HasValue && CreatedAfter.Value > CreatedBefore.Value)
            {
                yield return new ValidationResult(
                    "CreatedAfter cannot be later than CreatedBefore.",
                    new[] { nameof(CreatedAfter), nameof(CreatedBefore) });
            }
        }
    }
}