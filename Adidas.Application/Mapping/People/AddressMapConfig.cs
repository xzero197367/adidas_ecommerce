

using Adidas.DTOs.People.Address_DTOs;
using Mapster;
using Models.People;

namespace Adidas.Application.Mapping.People;

public class AddressMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<AddressDto, Address>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Address, AddressDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<AddressCreateDto, Address>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<AddressUpdateDto, Address>()
            .IgnoreNullValues(true);
    }
}