using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface ICategoryService : IGenericService<Category, CategoryResponseDto, CategoryCreateDto, CategoryUpdateDto>
    {
        // Category-specific methods
        Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentCategoryId);
        Task<CategoryResponseDto?> GetCategoryBySlugAsync(string slug);
        Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid categoryId);
        Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null);
        Task<PagedResultDto<CategoryDto>> GetPaginatedCategoryListAsync(int pageNumber, int pageSize);
    }
}
