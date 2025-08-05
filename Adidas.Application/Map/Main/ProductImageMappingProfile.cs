using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductImageMappingProfile: BaseMappingProfile
{
    public ProductImageMappingProfile()
    {            
        // ProductImage <=> DTOs
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductImageCreateDto, ProductImage>();
        CreateMap<UpdateProductImageDto, ProductImage>();
    }
}