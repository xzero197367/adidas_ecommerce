
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Mapster;

namespace Adidas.Application.Mapping.Operation;

public class OrderMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderDto, Order>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Order, OrderDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderCreateDto, Order>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<OrderUpdateDto, Order>()
            .IgnoreNullValues(true);
    }
}