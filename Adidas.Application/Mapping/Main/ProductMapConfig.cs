
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductMapConfig
{
    public static void Configure()
    {
        // Model to DTO - ignore circular nav
        TypeAdapterConfig.GlobalSettings.NewConfig<Product, ProductDto>()
            .Ignore(dest => dest.Variants);

        // DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductDto, Product>()
            .IgnoreNullValues(true);

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductCreateDto, Product>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductUpdateDto, Product>()
            .IgnoreNullValues(true);
    }
}
