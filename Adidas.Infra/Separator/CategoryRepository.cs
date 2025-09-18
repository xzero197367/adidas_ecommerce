using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Models.Separator;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Infra.Separator
{
    public class CategoryRepository : GenericRepository<Category>,ICategoryRepository

    {
        public CategoryRepository(AdidasDbContext context) : base(context)
        {
            _dbSet = context.Set<Category>();
        }

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
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted) 
                .ToListAsync();
        }



        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            // get all gategories including its all related entities 
            return await _dbSet
 
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.Products)
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted && c.IsActive);

        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            // get all gategories including its all related entities 

            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId );

 
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

        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await _dbSet
              .FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
