

using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartItemDto: BaseDto
    {
        public string UserId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        
        // navigation properties
        public ProductVariantDto Variant { get; set; }
    }
}
