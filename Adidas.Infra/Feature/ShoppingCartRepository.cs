using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Context;
using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Feature
{
    public class ShoppingCartRepository :GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(AdidasDbContext context) : base(context) { }
        public async Task<bool> RemoveFromCartAsync(string userId, Guid variantId)
        {
            var cartItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.VariantId == variantId);

            if (cartItem == null)
                return false;

            _context.ShoppingCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ShoppingCart>> GetCartItemsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(sc => sc.Variant)
                .ThenInclude(v => v.Product)
                .Where(sc => sc.UserId == userId)
                .ToListAsync();
        }

        public async Task<ShoppingCart?> GetCartItemAsync(string userId, Guid variantId)
        {
            return await _dbSet
                .Include(sc => sc.Variant)
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.VariantId == variantId);
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var items = await _dbSet.Where(sc => sc.UserId == userId).ToListAsync();
            if (!items.Any()) return false;

            _dbSet.RemoveRange(items);
            return true;
        }

        public async Task<decimal> CalculateTotalCostAsync(string userId)
        {
            var items = await _dbSet
                .Include(sc => sc.Variant)
                .Where(sc => sc.UserId == userId)
                .ToListAsync();

            return items.Sum(i => i.Variant.PriceAdjustment * i.Quantity);
        }

        public async Task<int> CountCartItemsAsync(string userId)
        {
            return await _dbSet.Where(sc => sc.UserId == userId).SumAsync(sc => sc.Quantity);
        }
    }
}