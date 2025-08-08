
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Models.People;

namespace Adidas.DTOs.Main.Product_DTOs
{
    public class CreateProductDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Sku { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public bool Sale { get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }

        [Required]
        public Gender GenderTarget { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public Guid BrandId { get; set; }

        public int SortOrder { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }

        public ICollection<CreateProductVariantDto> Variants { get; set; } = new List<CreateProductVariantDto>();
        public ICollection<CreateProductImageDto> Images { get; set; } = new List<CreateProductImageDto>();
    }
}
