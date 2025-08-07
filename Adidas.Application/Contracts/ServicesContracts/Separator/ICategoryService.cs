using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface ICategoryService : IGenericService<Category, CategoryDto, CategoryCreateDto, CategoryUpdateDto>
    {
        // Category-specific methods
        Task<OperationResult<IEnumerable<CategoryDto>>> GetMainCategoriesAsync();
        Task<OperationResult<bool>> CreateAsync(CategoryCreateDto createCategoryDto);
        Task<OperationResult<bool>> DeleteAsync(Guid id);
        Task<OperationResult<bool>> UpdateAsync(CategoryUpdateDto updateCategoryDto);
        Task<OperationResult<CategoryUpdateDto>> GetCategoryToEditByIdAsync(Guid id);
        Task<OperationResult<CategoryDto>> GetCategoryDetailsAsync(Guid id);
        //Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentCategoryId);
        //Task<CategoryResponseDto?> GetCategoryBySlugAsync(string slug);
        //Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid categoryId);
        //Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null);
        //Task<PagedResultDto<CategoryDto>> GetPaginatedCategoryListAsync(int pageNumber, int pageSize);
    }
}
