using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Context;
using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Repositories.Feature
{
    public class WishListRepository : GenericRepository<Wishlist>, IWishListRepository
    {
        public WishListRepository(AdidasDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Wishlist>> GetWishListByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .Include(w => w.Product)
                .ToListAsync();
        }

        public async Task<bool> IsProductInWishListAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);
        }

        public async Task<bool> RemoveFromWishListAsync(Guid userId, Guid productId)
        {
            var wishItem = await _dbSet
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);

            if (wishItem == null)
                return false;

            _dbSet.Remove(wishItem);
            //await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetWishListCountAsync(Guid userId)
        {
            return await _dbSet.CountAsync(w => w.UserId == userId && !w.IsDeleted);
        }
    }
}
