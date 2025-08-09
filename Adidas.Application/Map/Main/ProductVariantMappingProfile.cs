using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductVariantMappingProfile : BaseMappingProfile
{
    public ProductVariantMappingProfile()
    {
        // ProductVariant <=> DTOs
        CreateMap<ProductVariant, ProductVariantDto>()
             .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
    .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));
        CreateMap<CreateProductVariantDto, ProductVariant>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore()); // SKU may be generated
        CreateMap<UpdateProductVariantDto, ProductVariant>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore());
    }
}