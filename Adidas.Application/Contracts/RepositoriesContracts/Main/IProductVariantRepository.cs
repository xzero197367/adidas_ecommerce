using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IProductVariantRepository : IGenericRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);
        Task<ProductVariant?> GetVariantBySkuAsync(string sku);
        Task<IEnumerable<ProductVariant>> GetVariantsByColorAsync(string color);
        Task<IEnumerable<ProductVariant>> GetVariantsBySizeAsync(string size);
        Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(int threshold = 10);
        Task<bool> UpdateStockAsync(Guid variantId, int newStock);
        Task<bool> ReserveStockAsync(Guid variantId, int quantity);
        Task<bool> ReleaseStockAsync(Guid variantId, int quantity);
        Task<ProductVariant?> GetByIdWithImagesAsync(Guid id);
        void Remove(ProductVariant entity);
        Task<IEnumerable<ProductVariant>> GetAllWithProductAndImagesAsync();

        IQueryable<ProductVariant> GetAllForInventory(
    Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>>? queryFunc = null);
    }
}
