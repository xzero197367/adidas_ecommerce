
using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductAttributeMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeDto, ProductAttribute>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttribute, ProductAttributeDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeCreateDto, ProductAttribute>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeUpdateDto, ProductAttribute>()
            .IgnoreNullValues(true);
        
        // Model to Product attribute with values
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttribute, ProductAttributeWithValuesDto>();
    }
}