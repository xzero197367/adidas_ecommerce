using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.People.Address_DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.People;

public class AddressService : GenericService<Address, AddressDto, CreateAddressDto, UpdateAddressDto>, IAddressService
{
    public AddressService(IGenericRepository<Address> repository, IMapper mapper, ILogger logger) : base(repository,
        mapper, logger)
    {
    }

    public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(string userId)
    {
        var addresses = await _repository
            .GetAll(a => a.Where(a => a.UserId == userId && !a.IsDeleted).Include(a => a.User))
            .ToListAsync();

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto?> GetDefaultAddressAsync(string userId)
    {
        var defaultAddress = await
            _repository.FindAsync(q =>
                q.Where(a => a.IsDefault && a.UserId == userId && !a.IsDeleted).Include(a => a.User));
        return _mapper.Map<AddressDto>(defaultAddress);
    }

    public async Task<AddressDto> SetDefaultAddressAsync(Guid addressId, string userId)
    {
        var address = await _repository.GetByIdAsync(addressId);
        address.IsDefault = true;
        var updatedAddress = await _repository.UpdateAsync(address);
        // await _repository.SaveChangesAsync();
        return _mapper.Map<AddressDto>(updatedAddress.Entity);
    }

    public Task<bool> ValidateAddressAsync(CreateAddressDto addressDto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetSuggestedAddressesAsync(string partialAddress)
    {
        throw new NotImplementedException();
    }
}