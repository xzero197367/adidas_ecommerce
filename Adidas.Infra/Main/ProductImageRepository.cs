using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Context;
using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Main
{
    public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(AdidasDbContext context) : base(context) { }
        public async Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId && !pi.IsDeleted)
                .OrderBy(pi => pi.SortOrder)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductImage>> GetImagesByVariantIdAsync(Guid variantId)
        {
            return await _dbSet
                .Where(pi => pi.VariantId == variantId && !pi.IsDeleted)
                .OrderBy(pi => pi.SortOrder)
                .ToListAsync();
        }
        public async Task<ProductImage?> GetPrimaryImageAsync(Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsPrimary && !pi.IsDeleted);
        }
    }
}
