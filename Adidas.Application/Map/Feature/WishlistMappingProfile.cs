using Adidas.DTOs.Feature.WishLIstDTOS;
using Models.Feature;

namespace Adidas.Application.Map.Feature;

public class WishlistMappingProfile: BaseMappingProfile
{
    public WishlistMappingProfile()
    {
        // Wishlist <=> DTOs
        CreateMap<Wishlist, WishlistItemDto>();
        CreateMap<AddToWishlistDto, Wishlist>()
            .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

    }
}