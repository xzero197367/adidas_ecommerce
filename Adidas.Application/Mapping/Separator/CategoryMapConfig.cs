

using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;
using Mapster;
namespace Adidas.Application.Mapping.Separator;

public class CategoryMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CategoryDto, Category>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Category, CategoryDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CategoryCreateDto, Category>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CategoryUpdateDto, Category>()
            .IgnoreNullValues(true);
    }
}