using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.People.Address_DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.People;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _repository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository repository, ILogger<AddressService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<OperationResult<AddressDto>> CreateAsync(AddressCreateDto createDto)
    {
        try
        {
            // Manual mapping from CreateDto to Entity
            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = createDto.UserId.ToString(),
                StreetAddress = $"{createDto.Street} {createDto.Street2}".Trim(),
                City = createDto.City,
                StateProvince = createDto.State,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                IsDefault = createDto.IsDefault,
                AddressType = createDto.AddressType ?? "Home",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // If this address is set as default, ensure no other address for this user is default
            if (address.IsDefault)
            {
                await SetOtherAddressesAsNonDefaultAsync(address.UserId);
            }
            // If this is the user's first address, make it default
            else
            {
                var existingAddresses = await GetUserAddressesFromRepository(address.UserId);
                if (!existingAddresses.Any())
                {
                    address.IsDefault = true;
                }
            }

            var createdEntity = await _repository.AddAsync(address);
            await _repository.SaveChangesAsync();

            var result = MapToDto(createdEntity.Entity);
            return OperationResult<AddressDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address for user: {UserId}", createDto.UserId);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<AddressDto>> UpdateAsync(AddressUpdateDto updateDto)
    {
        try
        {
            var existingAddress = await _repository.GetByIdAsync(updateDto.Id);
            if (existingAddress == null)
            {
                return OperationResult<AddressDto>.Fail("Address not found");
            }

            // Check if address belongs to the user (if UserId is provided in updateDto)
            if (!string.IsNullOrEmpty(updateDto.UserId) && existingAddress.UserId != updateDto.UserId)
            {
                return OperationResult<AddressDto>.Fail("Address does not belong to the specified user");
            }

            // Manual mapping from UpdateDto to Entity - only update Address entity properties
            if (!string.IsNullOrEmpty(updateDto.Street))
            {
                var streetAddress = $"{updateDto.Street} {updateDto.Street2}".Trim();
                existingAddress.StreetAddress = streetAddress;
            }

            if (!string.IsNullOrEmpty(updateDto.City))
                existingAddress.City = updateDto.City;

            if (!string.IsNullOrEmpty(updateDto.State))
                existingAddress.StateProvince = updateDto.State;

            if (!string.IsNullOrEmpty(updateDto.PostalCode))
                existingAddress.PostalCode = updateDto.PostalCode;

            if (!string.IsNullOrEmpty(updateDto.Country))
                existingAddress.Country = updateDto.Country;

            if (updateDto.IsDefault.HasValue)
            {
                existingAddress.IsDefault = updateDto.IsDefault.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.AddressType))
            {
                existingAddress.AddressType = updateDto.AddressType;
            }

            existingAddress.UpdatedAt = DateTime.UtcNow;

            // If this address is being set as default, ensure no other address for this user is default
            if (existingAddress.IsDefault)
            {
                await SetOtherAddressesAsNonDefaultAsync(existingAddress.UserId, existingAddress.Id);
            }

            var updatedEntity = await _repository.UpdateAsync(existingAddress);
            await _repository.SaveChangesAsync();

            var result = MapToDto(updatedEntity.Entity);
            return OperationResult<AddressDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address with ID: {AddressId}", updateDto.Id);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var address = await _repository.GetByIdAsync(id);
            if (address == null)
            {
                return OperationResult<bool>.Fail("Address not found");
            }

            // Check if this is the default address
            bool wasDefault = address.IsDefault;
            string userId = address.UserId;

            // Soft delete the address
            await _repository.SoftDeleteAsync(id);
            await _repository.SaveChangesAsync();

            // If we deleted the default address, set another address as default if available
            if (wasDefault)
            {
                await SetNewDefaultAddressAsync(userId);
            }

            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address with ID: {AddressId}", id);
            return OperationResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<AddressDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var address = await _repository.GetByIdAsync(id);
            if (address == null || address.IsDeleted)
            {
                return OperationResult<AddressDto>.Fail("Address not found");
            }

            var result = MapToDto(address);
            return OperationResult<AddressDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address by ID: {AddressId}", id);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

    #endregion

    #region Custom Address Operations

    public async Task<OperationResult<IEnumerable<AddressDto>>> GetAddressesByUserIdAsync(string userId)
    {
        try
        {
            var addresses = await GetUserAddressesFromRepository(userId);
            var result = addresses.Select(MapToDto);
            return OperationResult<IEnumerable<AddressDto>>.Success(result);
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
            var defaultAddress = await GetDefaultAddressFromRepository(userId);
            if (defaultAddress == null)
            {
                return OperationResult<AddressDto>.Fail("No default address found for user");
            }

            var result = MapToDto(defaultAddress);
            return OperationResult<AddressDto>.Success(result);
        }
        catch (Exception ex)
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
            if (address == null || address.IsDeleted)
            {
                return OperationResult<AddressDto>.Fail("Address not found");
            }

            // Verify the address belongs to the user
            if (address.UserId != userId)
            {
                return OperationResult<AddressDto>.Fail("Address does not belong to the specified user");
            }

            // Set all user addresses to non-default first
            await SetOtherAddressesAsNonDefaultAsync(userId);

            // Set this address as default
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(address);
            await _repository.SaveChangesAsync();

            var result = MapToDto(address);
            return OperationResult<AddressDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address by user id: {UserId}, AddressId: {AddressId}", userId, addressId);
            return OperationResult<AddressDto>.Fail(ex.Message);
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<IEnumerable<Address>> GetUserAddressesFromRepository(string userId)
    {
        return await _repository
            .GetAll()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    private async Task<Address?> GetDefaultAddressFromRepository(string userId)
    {
        return await _repository
            .GetAll()
            .Where(a => a.UserId == userId && !a.IsDeleted && a.IsDefault)
            .FirstOrDefaultAsync();
    }

    private async Task SetOtherAddressesAsNonDefaultAsync(string userId, Guid? excludeAddressId = null)
    {
        try
        {
            var userAddresses = await GetUserAddressesFromRepository(userId);
            var addressesToUpdate = userAddresses.Where(a => a.IsDefault && a.Id != excludeAddressId).ToList();

            if (addressesToUpdate.Any())
            {
                foreach (var address in addressesToUpdate)
                {
                    address.IsDefault = false;
                    address.UpdatedAt = DateTime.UtcNow;
                }
                await _repository.UpdateRangeAsync(addressesToUpdate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting other addresses as non-default for user: {UserId}", userId);
            throw;
        }
    }

    private async Task SetNewDefaultAddressAsync(string userId)
    {
        try
        {
            var userAddresses = await GetUserAddressesFromRepository(userId);
            var firstAddress = userAddresses.FirstOrDefault();

            if (firstAddress != null)
            {
                firstAddress.IsDefault = true;
                firstAddress.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(firstAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting new default address for user: {UserId}", userId);
            throw;
        }
    }

    private AddressDto MapToDto(Address address)
    {
        // Split StreetAddress back to Street and Street2 for DTO compatibility
        var streetParts = address.StreetAddress?.Split(' ', 2) ?? new[] { "", "" };
        var street = streetParts.Length > 0 ? streetParts[0] : "";
        var street2 = streetParts.Length > 1 ? streetParts[1] : "";

        return new AddressDto
        {
            Id = address.Id,
            UserId = Guid.Parse(address.UserId),
            FirstName = "", // Not available in Address entity
            LastName = "", // Not available in Address entity
            Company = "", // Not available in Address entity
            Street = street,
            Street2 = street2,
            City = address.City,
            State = address.StateProvince ?? "",
            PostalCode = address.PostalCode,
            Country = address.Country,
            PhoneNumber = "", // Not available in Address entity
            IsDefault = address.IsDefault,
            AddressType = address.AddressType,
            FullAddress = $"{address.StreetAddress}, {address.City}, {address.StateProvince} {address.PostalCode}, {address.Country}",
            UpdatedAt = address.UpdatedAt,
        };
    }

    #endregion
}