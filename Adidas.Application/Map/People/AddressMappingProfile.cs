using Adidas.DTOs.People.Address_DTOs;
using Models.People;

namespace Adidas.Application.Map.People;

public class AddressMappingProfile : BaseMappingProfile
{
    public AddressMappingProfile()
    {
        // Address <=> DTOs
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => $"{src.AddedBy.UserName} "))
            .ForMember(dest => dest.FullAddress,
                opt => opt.MapFrom(src =>
                    $"{src.StreetAddress}, {src.City}, {src.StateProvince} {src.PostalCode}, {src.Country}"));
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();
    }
}