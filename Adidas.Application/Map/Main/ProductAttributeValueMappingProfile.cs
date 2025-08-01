using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductAttributeValueMappingProfile: BaseMappingProfile
{
    public ProductAttributeValueMappingProfile()
    {
        // ProductAttribute <=> DTOs
        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductAttributeValue, ProductAttributeValueDto>();
    }
}