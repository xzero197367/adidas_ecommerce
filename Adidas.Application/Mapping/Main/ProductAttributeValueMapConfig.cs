
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductAttributeValueMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeValueDto, ProductAttributeValue>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeValue, ProductAttributeValueDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeValueCreateDto, ProductAttributeValue>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductAttributeValueUpdateDto, ProductAttributeValue>()
            .IgnoreNullValues(true);
        
    }
}