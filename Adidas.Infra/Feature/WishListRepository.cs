using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Microsoft.EntityFrameworkCore;
using Models.Feature;

namespace Adidas.Infra.Repositories.Feature
{
    public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(w => w.Product)
                .ThenInclude(p => p.Images)
                .Include(w => w.Product)
                .ThenInclude(p => p.Brand)
                .Where(w => !w.IsDeleted && w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<bool> IsProductInWishlistAsync(string userId, Guid productId)
        {
            return await _dbSet.AnyAsync(w => !w.IsDeleted && w.UserId == userId && w.ProductId == productId);
        }

        public async Task<bool> RemoveFromWishlistAsync(string userId, Guid productId)
        {
            var wishlistItem = await _dbSet
                .FirstOrDefaultAsync(w => !w.IsDeleted && w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null) return false;

            await SoftDeleteAsync(wishlistItem.Id);
            return true;
        }

        public async Task<int> GetWishlistCountAsync(string userId)
        {
            return await _dbSet.CountAsync(w => !w.IsDeleted && w.UserId == userId);
        }

       
    }
}
