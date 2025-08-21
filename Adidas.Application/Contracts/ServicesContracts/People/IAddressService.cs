
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.People.Address_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People;

public interface IAddressService
{
    Task<OperationResult<AddressDto>> CreateAsync(AddressCreateDto createDto);
    Task<OperationResult<AddressDto>> UpdateAsync(AddressUpdateDto updateDto);
    Task<OperationResult<bool>> DeleteAsync(Guid id);
    Task<OperationResult<AddressDto>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<AddressDto>>> GetAddressesByUserIdAsync(string userId);
    Task<OperationResult<AddressDto>> GetDefaultAddressAsync(string userId);
    Task<OperationResult<AddressDto>> SetDefaultAddressAsync(Guid addressId, string userId);
   
}