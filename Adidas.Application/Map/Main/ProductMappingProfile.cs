using Adidas.DTOs.Main.Product_DTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Map.Main;

public class ProductMappingProfile: BaseMappingProfile
{
    public ProductMappingProfile()
    {
        // Product <=> DTOs
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.DisplayPrice, opt => opt.MapFrom(src => src.SalePrice ?? src.Price))
            .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => src.SalePrice.HasValue && src.SalePrice < src.Price))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
            .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.Variants.Any(v => v.StockQuantity > 0)));
        CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore()) // SKU is generated in ProductService
            .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
        CreateMap<ProductUpdateDto, Product>()
            .ForMember(dest => dest.Sku, opt => opt.Ignore()); // SKU is not updated


    }
}