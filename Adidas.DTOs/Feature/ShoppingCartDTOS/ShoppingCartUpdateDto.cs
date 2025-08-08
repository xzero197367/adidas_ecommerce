
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartUpdateDto: BaseUpdateDto
    {
        public int? Quantity { get; set; }
        public Guid? VariantId { get; set; }
    }
}
