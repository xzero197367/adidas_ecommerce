using Adidas.DTOs.People.Address_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People;

public interface IAddressService : IGenericService<Address, AddressDto, AddressCreateDto, AddressUpdateDto>
{
    Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(string userId);
    Task<AddressDto?> GetDefaultAddressAsync(string userId);
    Task<AddressDto> SetDefaultAddressAsync(Guid addressId, string userId);
    Task<bool> ValidateAddressAsync(AddressCreateDto dto);
    Task<IEnumerable<string>> GetSuggestedAddressesAsync(string partialAddress);
}