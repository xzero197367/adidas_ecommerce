
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.People.Address_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People;

public interface IAddressService : IGenericService<Address, AddressDto, AddressCreateDto, AddressUpdateDto>
{
    Task<OperationResult<IEnumerable<AddressDto>>> GetAddressesByUserIdAsync(string userId);
    Task<OperationResult<AddressDto>> GetDefaultAddressAsync(string userId);
    Task<OperationResult<AddressDto>> SetDefaultAddressAsync(Guid addressId, string userId);
    // Task<OperationResult<bool>> ValidateAddressAsync(AddressCreateDto dto);
    // Task<OperationResult<IEnumerable<string>>> GetSuggestedAddressesAsync(string partialAddress);
}