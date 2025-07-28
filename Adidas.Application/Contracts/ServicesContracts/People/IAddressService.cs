using Adidas.DTOs.People.Address_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People;

public interface IAddressService : IGenericService<Address, AddressDto, CreateAddressDto, UpdateAddressDto>
{
    Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(string userId);
    Task<AddressDto?> GetDefaultAddressAsync(string userId);
    Task<AddressDto> SetDefaultAddressAsync(Guid addressId, string userId);
    Task<bool> ValidateAddressAsync(CreateAddressDto addressDto);
    Task<IEnumerable<string>> GetSuggestedAddressesAsync(string partialAddress);
}