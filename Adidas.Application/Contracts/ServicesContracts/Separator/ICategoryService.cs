using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface ICategoryService 
    {
        // Category-specific methods
        Task<OperationResult<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync();
        Task<Result> CreateAsync(CategoryCreateDto createCategoryDto);
        Task<Result>DeleteAsync(Guid id);
        Task<Result> UpdateAsync(CategoryUpdateDto updateCategoryDto);
        Task<CategoryUpdateDto> GetCategoryToEditByIdAsync(Guid id);
        Task<CategoryDto> GetCategoryDetailsAsync(Guid id);
        Task<IEnumerable<CategoryDto>> GetFilteredCategoriesAsync(string categoryType, string statusFilter, string searchTerm);
        Task<Result> ToggleCategoryStatusAsync(Guid categoryId);


        // Task<IEnumerable<CategoryDto>> GetAllAsync();


        //Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentCategoryId);
        //Task<CategoryResponseDto?> GetCategoryBySlugAsync(string slug);
        //Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid categoryId);
        //Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null);
        //Task<PagedResultDto<CategoryDto>> GetPaginatedCategoryListAsync(int pageNumber, int pageSize);
    }
}
