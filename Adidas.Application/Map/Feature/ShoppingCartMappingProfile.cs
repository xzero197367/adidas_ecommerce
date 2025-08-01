using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;

namespace Adidas.Application.Map.Feature;

public class ShoppingCartMappingProfile : BaseMappingProfile
{
    public ShoppingCartMappingProfile()
    {
        // ShoppingCart <=> DTOs
        CreateMap<ShoppingCart, ShoppingCartItemDto>()
            .ForMember(dest => dest.UnitPrice,
                opt => opt.MapFrom(src =>
                    (src.Variant.Product.SalePrice ?? src.Variant.Product.Price) + src.Variant.PriceAdjustment))
            .ForMember(dest => dest.TotalPrice,
                opt => opt.MapFrom(src =>
                    ((src.Variant.Product.SalePrice ?? src.Variant.Product.Price) + src.Variant.PriceAdjustment) *
                    src.Quantity));
        CreateMap<AddToCartDto, ShoppingCart>()
            .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UpdateCartItemDto, ShoppingCart>();
    }
}