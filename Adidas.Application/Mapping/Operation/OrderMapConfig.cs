using System.Text.Json;
using Mapster;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;

namespace Adidas.Application.Mapping.Operation;

public class OrderMapConfig
{
    public static void Configure()
    {
        var jsonOptions = new JsonSerializerOptions();

        TypeAdapterConfig<Order, OrderDto>.NewConfig()
            .Map(dest => dest.ShippingAddress,
                src => JsonSerializer.Deserialize<Dictionary<string, object>>(src.ShippingAddress, jsonOptions))
            .Map(dest => dest.BillingAddress,
                src => JsonSerializer.Deserialize<Dictionary<string, object>>(src.BillingAddress, jsonOptions));

        TypeAdapterConfig<OrderDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress,
                src => JsonSerializer.Serialize(src.ShippingAddress, jsonOptions))
            .Map(dest => dest.BillingAddress,
                src => JsonSerializer.Serialize(src.BillingAddress, jsonOptions))
            .IgnoreNullValues(true);

        TypeAdapterConfig<OrderCreateDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress,
                src => JsonSerializer.Serialize(src.ShippingAddress, jsonOptions))
            .Map(dest => dest.BillingAddress,
                src => JsonSerializer.Serialize(src.BillingAddress, jsonOptions))
            .IgnoreNullValues(true);

       

        TypeAdapterConfig<OrderUpdateDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress,
                src => JsonSerializer.Serialize(src.ShippingAddress, jsonOptions))
            .Map(dest => dest.BillingAddress,
                src => JsonSerializer.Serialize(src.BillingAddress, jsonOptions))
            .IgnoreNullValues(true);
    }
}