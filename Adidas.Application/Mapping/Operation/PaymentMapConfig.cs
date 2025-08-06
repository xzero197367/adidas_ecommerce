
using Adidas.DTOs.Operation.PaymentDTOs;
using Mapster;

namespace Adidas.Application.Mapping.Operation;

public class PaymentMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<PaymentDto, Payment>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Payment, PaymentDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<PaymentCreateDto, Payment>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<PaymentUpdateDto, Payment>()
            .IgnoreNullValues(true);
    }
}