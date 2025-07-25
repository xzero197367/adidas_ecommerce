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
    public class ProductAttributeRepository : GenericRepository<ProductAttribute>, IProductAttributeRepository
    {
        
        public ProductAttributeRepository(AdidasDbContext context) : base(context) { }
        
        public async Task<IEnumerable<ProductAttribute>> GetFilterableAttributesAsync()
        {
            return await _dbSet
                .Where(pa => pa.IsFilterable && !pa.IsDeleted)
                .OrderBy(pa => pa.SortOrder)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductAttribute>> GetRequiredAttributesAsync()
        {
            return await _dbSet
                .Where(pa => pa.IsRequired && !pa.IsDeleted)
                .OrderBy(pa => pa.SortOrder)
                .ToListAsync();
        }
    }
}
