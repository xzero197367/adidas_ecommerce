using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductAttributeMappingProfile:BaseMappingProfile
{
    public ProductAttributeMappingProfile()
    {
        // ProductAttribute <=> DTOs
        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<CreateProductAttributeDto, ProductAttribute>();
        CreateMap<UpdateProductAttributeDto, ProductAttribute>();
    }
}