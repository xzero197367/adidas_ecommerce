using Adidas.DTOs.People.UserDtos;
using Models.People;

namespace Adidas.Application.Map.People;

public class UserMappingProfile : BaseMappingProfile
{
    public UserMappingProfile()
    {
        // User <=> DTOs
        CreateMap<User, UserGetDto>()
            .ForMember(src => src.Id, opt => opt.MapFrom(dest => dest.Id))
            .ForMember(src => src.UserName, opt => opt.MapFrom(dest => dest.UserName))
            .ForMember(src => src.Email, opt => opt.MapFrom(dest => dest.Email))
            .ForMember(src => src.PhoneNumber, opt => opt.MapFrom(dest => dest.PhoneNumber))
            .ForMember(src => src.FirstName, opt => opt.MapFrom(dest => dest.FirstName))
            .ForMember(src => src.LastName, opt => opt.MapFrom(dest => dest.LastName))
            .ForMember(src => src.Role, opt => opt.MapFrom(dest => dest.Role));

    }
}