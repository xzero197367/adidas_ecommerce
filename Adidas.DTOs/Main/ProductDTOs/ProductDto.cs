using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Models.People;

namespace Adidas.DTOs.Main.ProductDTOs
{
    public class ProductDto : BaseDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ShortDescription { get; set; }
        public required decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public required Gender GenderTarget { get; set; }
    
        public string? MetaTitle { get; set; }

        public string? MetaDescription { get; set; }
        public required string Sku { get; set; }

        public string? Specifications { get; set; }
        
        // calculated properties
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // foreign keys
        public required Guid CategoryId { get; set; }
        public required Guid BrandId { get; set; }
        
        // navigations
        public CategoryDto Category { get; set; }
        public BrandDto Brand { get; set; }
        
        public ICollection<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }
}
