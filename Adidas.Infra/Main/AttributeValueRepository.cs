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
    public class AttributeValueRepository : GenericRepository<ProductAttributeValue>, IAttributeValueRepository
    {
        public AttributeValueRepository(AdidasDbContext context) : base(context) { }
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
