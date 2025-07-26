using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Context;
using Microsoft.EntityFrameworkCore;
using Models.People;

namespace Adidas.Infra.People;

public class AddressRepository : GenericRepository<Address>, IAddressRepository
{
    public AddressRepository(AdidasDbContext context) : base(context) { }

    public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(a => !a.IsDeleted && a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Address?> GetDefaultAddressAsync(string userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => !a.IsDeleted && a.UserId == userId && a.IsDefault);
    }

    public async Task<bool> SetDefaultAddressAsync(string userId, Guid addressId)
    {
        // Remove default from all user addresses
        var userAddresses = await _dbSet
            .Where(a => !a.IsDeleted && a.UserId == userId)
            .ToListAsync();

        foreach (var address in userAddresses)
        {
            address.IsDefault = address.Id == addressId;
        }

        await UpdateRangeAsync(userAddresses);
        return true;
    }
}
