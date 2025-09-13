using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Operation;
using Models.People;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.Product_DTOs
{
    public class ProductDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public Gender GenderTarget { get; set; }
        public int SortOrder { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
        public string CategoryName { get; set; } = "No Category";
        public string BrandName { get; set; } = "No Brand";
        public bool InStock { get; set; }
        public bool ComputedInStock => Variants != null && Variants.Any(v => v.StockQuantity > 0);
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SalePrice.HasValue && SalePrice > Price)
            {
                yield return new ValidationResult(
                    "Sale Price cannot be greater than the original Price.",
                    new[] { nameof(SalePrice) });
            }
        }
        // Navigation properties
        public CategoryDto Category { get; set; } = new();
        public BrandDto Brand { get; set; } = new();
        public ICollection<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Computed properties
        public decimal DisplayPrice => SalePrice ?? Price;
        public bool IsOnSale => SalePrice.HasValue;
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public int ReviewCount => Reviews.Count;
    }
}
