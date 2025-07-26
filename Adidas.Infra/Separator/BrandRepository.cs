using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Context;
using Adidas.Models.Separator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Separator
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        public BrandRepository(AdidasDbContext context) : base(context) { }

        public async Task<Brand?> GetBrandByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == name && !b.IsDeleted);
        }

        public async Task<IEnumerable<Brand>> GetPopularBrandsAsync()
        {
            return await _dbSet
                .Where(b => !b.IsDeleted && b.IsActive)
                .OrderByDescending(b => b.CreatedAt) // Simulating popularity
                .Take(10)
                .ToListAsync();
        }
    }
}
