using Adidas.DTOs.Main.ProductAttributeValueDTOs;

namespace Adidas.DTOs.Main.ProductAttributeDTOs;

public class ProductAttributeWithValuesDto: ProductAttributeDto
{
    public List<ProductAttributeValueDto> AttributeValues { get; set; } = new List<ProductAttributeValueDto>();
}