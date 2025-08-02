
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;

namespace Adidas.DTOs.Main.Product_Variant_DTOs
{
    public class ProductVariantDto : BaseDto
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal PriceAdjustment { get; set; }
        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }

        public ProductDto Product { get; set; } = new();
        public ICollection<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
    }
}
