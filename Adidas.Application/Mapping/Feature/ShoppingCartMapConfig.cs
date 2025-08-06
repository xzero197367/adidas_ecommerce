using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;
using Mapster;

namespace Adidas.Application.Mapping.Feature;

public class ShoppingCartMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartDto, ShoppingCart>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCart, ShoppingCartDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartCreateDto, ShoppingCart>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartUpdateDto, ShoppingCart>()
            .IgnoreNullValues(true);
        
        // Model to Cart Summary
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCart, ShoppingCartSummaryDto>();
    }
}