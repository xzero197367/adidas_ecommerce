

using Adidas.DTOs.People.UserDtos;
using Mapster;
using Models.People;

namespace Adidas.Application.Mapping.People;

public class UserMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<UserDto, User>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<User, UserDto>();

        // Create DTO to Model
        // TypeAdapterConfig.GlobalSettings.NewConfig<UserCreateDto, User>()
            // .IgnoreNullValues(true);

        // Update DTO to Model
        // TypeAdapterConfig.GlobalSettings.NewConfig<UserUpdateDto, User>()
        //     .IgnoreNullValues(true);
    }
}