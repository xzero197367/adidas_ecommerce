
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductImageMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductImageDto, ProductImage>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductImage, ProductImageDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductImageCreateDto, ProductImage>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductImageUpdateDto, ProductImage>()
            .IgnoreNullValues(true);
    }
}