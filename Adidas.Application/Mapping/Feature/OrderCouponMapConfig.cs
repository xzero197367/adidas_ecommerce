using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;
using Mapster;

namespace Adidas.Application.Mapping.Feature;

public class OrderCouponMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderCouponDto, OrderCoupon>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderCoupon, OrderCouponDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderCouponCreateDto, OrderCoupon>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderCouponUpdateDto, OrderCoupon>()
            .IgnoreNullValues(true);
    }
}