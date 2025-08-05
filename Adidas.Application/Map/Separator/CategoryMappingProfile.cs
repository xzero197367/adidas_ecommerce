using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Map.Separator;

public class CategoryMappingProfile: BaseMappingProfile
{
    public CategoryMappingProfile()
    {
        // Category <=> DTOs
        CreateMap<Category, CategoryDto>()
  .ForMember(dest => dest.HasSubCategories, opt => opt.MapFrom(src => src.SubCategories.Any()))
  .ForMember(dest => dest.ProductsCount, opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore()); // Slug is generated
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore());
            CreateMap<CreateCategoryDto, Category>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
               .ForMember(dest => dest.AddedById, opt => opt.Ignore())
               .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
               .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
               .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
               .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.AddedById, opt => opt.Ignore())
                .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ForMember(dest => dest.SubCategoryCount, opt => opt.MapFrom(src => src.SubCategories != null ? src.SubCategories.Count : 0))
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore()); // Will be set manually in service

            CreateMap<Category, CategoryHierarchyDto>()
                .ForMember(dest => dest.Level, opt => opt.Ignore()); // Will be set manually in service
            CreateMap<Category, CategoryListDto>();
            CreateMap<CategoryListDto, CategoryDto>();
    }
}