using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.Product_Variant_DTOs
{
    public class ProductVariantCreateDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required, MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Size { get; set; } = string.Empty;

        [Required, Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public decimal PriceAdjustment { get; set; } = 0;

        public string? ColorHex { get; set; }

        public int SortOrder { get; set; }
        
        public IFormFile? ImageFile { get; set; }
      
        public string? ImageUrl { get; set; }
    }
}
