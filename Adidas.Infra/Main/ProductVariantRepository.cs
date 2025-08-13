using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Context;
using Adidas.Models.Main;

namespace Adidas.Infra.Main
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(AdidasDbContext context) : base(context) { }

        public IQueryable<ProductVariant> GetAllForInventory(
    Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>>? queryFunc = null)
        {
            IQueryable<ProductVariant> query = _dbSet.AsNoTracking();

            if (queryFunc != null)
            {
                query = queryFunc(query); // Still EF query if you only use EF methods
            }

            return query; // Return EF IQueryable, not enumerated yet
        }
        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Include(v => v.Images)
                .Where(v => !v.IsDeleted && v.IsActive && v.ProductId == productId)
                .ToListAsync();
        }
        public void Remove(ProductVariant entity)
        {
            _context.ProductVariants.Remove(entity);
        }

        public async Task<ProductVariant?> GetByIdWithImagesAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.Images)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }
        public async Task<ProductVariant?> GetVariantBySkuAsync(string sku)
        {
            return await _dbSet
                .Include(v => v.Product)
                .Include(v => v.Images)
                .FirstOrDefaultAsync(v => !v.IsDeleted && v.Sku == sku);
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByColorAsync(string color)
        {
            return await _dbSet
                .Include(v => v.Product)
                .Where(v => !v.IsDeleted && v.IsActive && v.Color == color)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsBySizeAsync(string size)
        {
            return await _dbSet
                .Include(v => v.Product)
                .Where(v => !v.IsDeleted && v.IsActive && v.Size == size)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(int threshold = 10)
        {
            return await _dbSet
                .Include(v => v.Product)
                .Where(v => !v.IsDeleted && v.IsActive && v.StockQuantity <= threshold)
                .ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(Guid variantId, int newStock)
        {
            var variant = await GetByIdAsync(variantId);
            if (variant == null) return false;

            variant.StockQuantity = newStock;
            await UpdateAsync(variant);
            return true;
        }

        public async Task<bool> ReserveStockAsync(Guid variantId, int quantity)
        {
            var variant = await GetByIdAsync(variantId);
            if (variant == null || variant.StockQuantity < quantity) return false;

            variant.StockQuantity -= quantity;
            await UpdateAsync(variant);
            return true;
        }

        public async Task<bool> ReleaseStockAsync(Guid variantId, int quantity)
        {
            var variant = await GetByIdAsync(variantId);
            if (variant == null) return false;

            variant.StockQuantity += quantity;
            await UpdateAsync(variant);
            return true;
        }

        public async Task<IEnumerable<ProductVariant>> GetAllWithProductAndImagesAsync()
        {
            return await _dbSet
                .Include(v => v.Product)
                .Include(v => v.Images)
                .ToListAsync();
        }


    }
}
