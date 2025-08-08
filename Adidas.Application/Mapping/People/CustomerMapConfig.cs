

using Adidas.DTOs.People.Customer_DTOs;
using Mapster;
using Models.People;

namespace Adidas.Application.Mapping.People;

public class CustomerMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CustomerDto, User>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<User, CustomerDto>();

        // Create DTO to Model
        // TypeAdapterConfig.GlobalSettings.NewConfig<CustomerCreateDto, Customer>()
            // .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<CustomerUpdateDto, User>()
            .IgnoreNullValues(true);
    }
}