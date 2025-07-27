using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface ICategoryService : IGenericService<Category, CategoryResponseDto, CreateCategoryDto, UpdateCategoryDto>
    {
        // Category-specific methods
        Task<IEnumerable<CategoryListDto>> GetMainCategoriesAsync();
        Task<IEnumerable<CategoryListDto>> GetSubCategoriesAsync(Guid parentCategoryId);
        Task<CategoryResponseDto?> GetCategoryBySlugAsync(string slug);
        Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid categoryId);
        Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null);
        Task<PagedResultDto<CategoryListDto>> GetPaginatedCategoryListAsync(int pageNumber, int pageSize);
    }
}
