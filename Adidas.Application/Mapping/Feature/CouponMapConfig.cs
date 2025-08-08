using Adidas.DTOs.Feature.CouponDTOs;
using Mapster;
using Models.Feature;

namespace Adidas.Application.Mapping.Feature;

public class CouponMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CouponDto, Coupon>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Coupon, CouponDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CouponCreateDto, Coupon>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CouponUpdateDto, Coupon>()
            .IgnoreNullValues(true);
    }
}