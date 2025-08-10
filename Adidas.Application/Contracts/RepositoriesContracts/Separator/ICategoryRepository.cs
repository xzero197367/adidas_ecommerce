using Adidas.Models.Separator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Separator
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetMainCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId);
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<List<Category>> GetCategoryHierarchyAsync(Guid categoryId);
        Task<Category> GetCategoryByNameAsync(string name);
       
    }
}
