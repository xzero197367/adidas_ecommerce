using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Map.Separator;

public class BrandMappingProfile: BaseMappingProfile
{
    public BrandMappingProfile()
    {
        // Brand <=> DTOs

        CreateMap<CreateBrandDto, Brand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.AddedById, opt => opt.Ignore())
            .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        CreateMap<UpdateBrandDto, Brand>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.AddedById, opt => opt.Ignore())
            .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        CreateMap<Brand, BrandDto>();
      


        CreateMap<Brand, BrandResponseDto>();
        CreateMap<Brand, BrandListDto>();
    }
}