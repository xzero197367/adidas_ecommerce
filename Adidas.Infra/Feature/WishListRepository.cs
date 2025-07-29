using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using System.Data.Entity;
using Models.Feature;

namespace Adidas.Infra.Repositories.Feature
{
    public class WishListRepository : GenericRepository<Wishlist>, IWishListRepository
    {
        public WishListRepository(AdidasDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Wishlist>> GetWishListByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .Include(w => w.Product)
                .ToListAsync();
        }

        public async Task<bool> IsProductInWishListAsync(string userId, Guid productId)
        {
            return await _dbSet
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);
        }

        public async Task<bool> RemoveFromWishListAsync(string userId, Guid productId)
        {
            var wishItem = await _dbSet
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);

            if (wishItem == null)
                return false;

            _dbSet.Remove(wishItem);
            //await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetWishListCountAsync(string userId)
        {
            return await _dbSet.CountAsync(w => w.UserId == userId && !w.IsDeleted);
        }
    }
}
