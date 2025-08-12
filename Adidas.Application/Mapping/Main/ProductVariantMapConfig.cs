
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductVariantMapConfig
{
    public static void Configure()
    {
        // Model to DTO - ignore circular nav
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductVariant, ProductVariantDto>()
            .Ignore(dest => dest.Product);

        // DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductVariantDto, ProductVariant>()
            .IgnoreNullValues(true);

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductVariantCreateDto, ProductVariant>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductVariantUpdateDto, ProductVariant>()
            .IgnoreNullValues(true);
    }
}
