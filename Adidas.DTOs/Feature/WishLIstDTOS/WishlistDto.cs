

using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.ProductDTOs;

namespace Adidas.DTOs.Feature.WishLIstDTOS
{
    public class WishlistDto: BaseDto
    {
        public string UserId { get; set; }
        public Guid ProductId { get; set; }
        
        // navigation properties
        public ProductDto Product { get; set; }
    }
}