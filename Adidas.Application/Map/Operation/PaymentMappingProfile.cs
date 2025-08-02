using Adidas.DTOs.Operation.PaymentDTOs.Result;

namespace Adidas.Application.Map.Operation;

public class PaymentMappingProfile: BaseMappingProfile
{
    public PaymentMappingProfile()
    {
        // Payment <=> DTOs
        CreateMap<Payment, PaymentDto>();
    }
}