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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetMainCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == null && !c.IsDeleted && c.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId)
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted && c.IsActive)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted && c.IsActive);
        }

        public async Task<List<Category>> GetCategoryHierarchyAsync(Guid categoryId)
        {
            var hierarchy = new List<Category>();
            var current = await _dbSet.FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted);

            while (current != null)
            {
                hierarchy.Insert(0, current);
                if (current.ParentCategoryId == null) break;
                current = await _dbSet.FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId && !c.IsDeleted);
            }

            return hierarchy;
        }
    }
}
