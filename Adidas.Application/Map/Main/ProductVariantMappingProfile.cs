using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductVariantMappingProfile : BaseMappingProfile
{
    public ProductVariantMappingProfile()
    {
        // ProductVariant <=> DTOs
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.ColorHex,
                opt => opt.MapFrom(src => src.ImageUrl)); // Assuming ImageUrl might store ColorHex
        CreateMap<CreateProductVariantDto, ProductVariant>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore()); // SKU may be generated
        CreateMap<ProductVariantUpdateDto, ProductVariant>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore());
    }
}