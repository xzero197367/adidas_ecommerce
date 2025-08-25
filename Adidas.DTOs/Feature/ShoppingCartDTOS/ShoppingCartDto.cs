using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartDto : BaseDto
    {
        public string UserId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }

        // Navigation properties
        public ProductVariantDto Variant { get; set; }

        // Calculated properties
        public decimal TotalPrice => Quantity * Variant.Product.SalePrice ?? Variant.Product.Price;
        public bool IsAvailable => Variant != null && Variant.StockQuantity >= Quantity; // Added null check
    }
}