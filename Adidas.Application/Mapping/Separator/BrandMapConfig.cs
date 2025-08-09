

using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;
using Mapster;
namespace Adidas.Application.Mapping.Separator;

public class BrandMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<BrandDto, Brand>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Brand, BrandDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<BrandCreateDto, Brand>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<BrandUpdateDto, Brand>()
            .IgnoreNullValues(true);
    }
}