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
         .Map(dest => dest.ShippingAddress, src => src.ShippingAddress)
         .Map(dest => dest.BillingAddress, src => src.BillingAddress);

        TypeAdapterConfig<OrderDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress, src => src.ShippingAddress)
            .Map(dest => dest.BillingAddress, src => src.BillingAddress)
            .IgnoreNullValues(true);

        TypeAdapterConfig<OrderCreateDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress, src => src.ShippingAddress)
            .Map(dest => dest.BillingAddress, src => src.BillingAddress)
            .IgnoreNullValues(true);

        TypeAdapterConfig<OrderUpdateDto, Order>.NewConfig()
            .Map(dest => dest.ShippingAddress, src => src.ShippingAddress)
            .Map(dest => dest.BillingAddress, src => src.BillingAddress)
            .IgnoreNullValues(true);

    }
}