using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.People.Address_DTOs;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.People;

public class AddressService : GenericService<Address, AddressDto, AddressCreateDto, AddressUpdateDto>,IAddressService
{
    private readonly IAddressRepository _repository;
    private readonly ILogger _logger;

    public AddressService(IAddressRepository repository, ILogger logger): base(repository, logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OperationResult<IEnumerable<AddressDto>>> GetAddressesByUserIdAsync(string userId)
    {
        try
        {
            var addresses = await _repository
                .GetAll(a => a.Where(a => a.UserId == userId && !a.IsDeleted).Include(a => a.User))
                .ToListAsync();

            return OperationResult<IEnumerable<AddressDto>>.Success(addresses.Adapt<IEnumerable<AddressDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting addresses by user id: {UserId}", userId);
            return OperationResult<IEnumerable<AddressDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<AddressDto>> GetDefaultAddressAsync(string userId)
    {
        try
        {
            var defaultAddress = await
                _repository.FindAsync(q =>
                    q.Where(a => a.IsDefault && a.UserId == userId && !a.IsDeleted).Include(a => a.User));
            
            return OperationResult<AddressDto>.Success(defaultAddress.Adapt<AddressDto>());
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address by user id: {UserId}", userId);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<AddressDto>> SetDefaultAddressAsync(Guid addressId, string userId)
    {
        try
        {
            var address = await _repository.GetByIdAsync(addressId);
            address.IsDefault = true;
            var updatedAddress = await _repository.UpdateAsync(address);
            // await _repository.SaveChangesAsync();
           
            return OperationResult<AddressDto>.Success(updatedAddress.Adapt<AddressDto>());
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address by user id: {UserId}", userId);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

   
}