using Adidas.DTOs.Feature.WishLIstDTOS;
using Mapster;
using Models.Feature;

namespace Adidas.Application.Mapping.Feature;

public class WishlistMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<WishlistDto, Wishlist>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Wishlist, WishlistDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<WishlistCreateDto, Wishlist>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<WishlistUpdateDto, Wishlist>()
            .IgnoreNullValues(true);
    }
}