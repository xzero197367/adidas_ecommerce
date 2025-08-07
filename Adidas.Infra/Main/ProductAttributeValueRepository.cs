using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Models.Main;
using System.Data.Entity;

namespace Adidas.Infra.Main
{
    public class ProductAttributeValueRepository : GenericRepository<ProductAttributeValue>, IProductAttributeValueRepository
    {
        public ProductAttributeValueRepository(AdidasDbContext context) : base(context) { }
        public async Task<IEnumerable<ProductAttributeValue>> GetValuesByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Where(av => av.ProductId == productId && !av.IsDeleted)
                .OrderBy(av => av.Attribute.SortOrder)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductAttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            return await _dbSet
                .Where(av => av.AttributeId == attributeId && !av.IsDeleted)
                .OrderBy(av => av.Product.Name)
                .ToListAsync();
        }
        public async Task<ProductAttributeValue?> GetValueAsync(Guid valueId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(av => av.Id == valueId && !av.IsDeleted);
        }
    }
}
