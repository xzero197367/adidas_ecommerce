using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Context;
using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Feature
{
    public class ShoppingCartRepository :GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<ShoppingCart>> GetCartItemsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(sc => sc.Variant)
                .Include(sc => sc.Variant.Product)
                .Where(sc => sc.UserId == userId)
                .ToListAsync();
        }

        public async Task<ShoppingCart?> GetCartItemAsync(Guid userId, Guid variantId)
        {
            return await _dbSet
                .Include(sc => sc.Variant)
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.VariantId == variantId);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var items = await _dbSet.Where(sc => sc.UserId == userId).ToListAsync();
            if (!items.Any()) return false;

            _dbSet.RemoveRange(items);
            return true;
        }

        public async Task<decimal> CalculateTotalCostAsync(Guid userId)
        {
            var items = await _dbSet
                .Include(sc => sc.Variant)
                .Where(sc => sc.UserId == userId)
                .ToListAsync();

            return items.Sum(i => i.Variant.PriceAdjustment * i.Quantity);
        }

        public async Task<int> CountCartItemsAsync(Guid userId)
        {
            return await _dbSet.Where(sc => sc.UserId == userId).SumAsync(sc => sc.Quantity);
        }
    }
}