
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.Models.Main;
using Mapster;

namespace Adidas.Application.Mapping.Main;

public class ProductMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductDto, Product>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Product, ProductDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductCreateDto, Product>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductUpdateDto, Product>()
            .IgnoreNullValues(true);
    }
}