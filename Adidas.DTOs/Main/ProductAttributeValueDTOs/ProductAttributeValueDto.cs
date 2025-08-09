using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.DTOs.Main.ProductDTOs;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class ProductAttributeValueDto: BaseDto
    {
        public required string Value { get; set; }
        // foreign key
        public required Guid ProductId { get; set; }
        public required Guid AttributeId { get; set; }

        // Navigation properties as DTOs
        public ProductAttributeDto? Attribute { get; set; }
        public ProductDto? Product { get; set; }
    }
}
