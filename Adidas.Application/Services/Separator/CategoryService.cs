using System.Data.Entity;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Models.Separator;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.DTOs.Common_DTOs;
using Microsoft.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Mapster;
using Adidas.DTOs.Main.Product_DTOs;
using Microsoft.EntityFrameworkCore.Query;

namespace Adidas.Application.Services.Separator
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(
            ICategoryRepository categoryRepository
        )
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<OperationResult<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            try
            {

                var entities = await _categoryRepository.GetAllAsync();
                var dtos = entities.Adapt<IEnumerable<CategoryDto>>();

                return OperationResult<IEnumerable<CategoryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.Fail("Error getting all entities: " + ex.Message);
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var mainCategories = categories
                .Where(c => c.ParentCategoryId == null && c.IsActive && !c.IsDeleted)
                .ToList();

            var categoryDtos = mainCategories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder,
                ParentCategoryId = c.ParentCategoryId,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = c.UpdatedAt,
                IsActive = c.IsActive,
                Products = c.Products?.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                }).ToList() ?? new List<ProductDto>(),
                SubCategories = c.SubCategories?.Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    Slug = sc.Slug,
                    ImageUrl = sc.ImageUrl,
                    SortOrder = sc.SortOrder,
                    ParentCategoryId = sc.ParentCategoryId,
                    CreatedAt = sc.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = sc.UpdatedAt,
                    IsActive = sc.IsActive
                }).ToList() ?? new List<CategoryDto>()
            }).ToList();

            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetFilteredCategoriesAsync(string categoryType, string statusFilter,
            string searchTerm)
        {
            var categories = await _categoryRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(categoryType))
            {
                if (categoryType == "Main")
                    categories = categories.Where(c => c.ParentCategoryId == null).ToList();
                else if (categoryType == "Sub")
                    categories = categories.Where(c => c.ParentCategoryId != null).ToList();
            }


            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "Active";
                categories = categories.Where(c => c.IsActive == isActive).ToList();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = categories.Where(c =>
                    c.Name != null && c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }


            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder,
                ParentCategoryId = c.ParentCategoryId,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = c.UpdatedAt,
                IsActive = c.IsActive,
                Products = c.Products?.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList() ?? new List<ProductDto>(),
                SubCategories = c.SubCategories?.Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    Slug = sc.Slug,
                    ImageUrl = sc.ImageUrl,
                    SortOrder = sc.SortOrder,
                    ParentCategoryId = sc.ParentCategoryId,
                    CreatedAt = sc.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = sc.UpdatedAt,
                    IsActive = sc.IsActive
                }).ToList() ?? new List<CategoryDto>()
            }).ToList();

            return categoryDtos;
        }

        public async Task<Result> CreateAsync(CategoryCreateDto createCategoryDto)
        {
            try
            {
                var category = new Category
                {
                    ParentCategoryId = createCategoryDto.ParentCategoryId,
                    Name = createCategoryDto.Name,
                    Slug = createCategoryDto.Slug,
                    SortOrder = createCategoryDto.SortOrder,
                    Description = createCategoryDto.Description,
                    ImageUrl = createCategoryDto.ImageUrl,
                };

                await _categoryRepository.AddAsync(category);
                var result = await _categoryRepository.SaveChangesAsync();

                return result == null
                    ? Result.Failure("Could not save the category.")
                    : Result.Success();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx &&
                    sqlEx.Message.Contains("IX_Categories_Slug"))
                {
                    return Result.Failure("A category with the same slug already exists.");
                }

                return Result.Failure("A database error occurred.");
            }
            catch (Exception)
            {
                return Result.Failure("An unexpected error occurred.");
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(
                id,
                c => c.Products,
                c => c.SubCategories);
            if (category == null)
                return Result.Failure("Category not found.");

            //var Children = await _categoryRepository.GetSubCategoriesAsync(id);

            if (category.SubCategories.Count() != 0)
                return Result.Failure("Cannot delete a category that has subcategories.");
            if (category.Products.Count() != 0)
                return Result.Failure("Cannot delete a category that has products.");

            await _categoryRepository.HardDeleteAsync(id);
            var result = await _categoryRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to delete category.") : Result.Success();
        }

        public async Task<Result> UpdateAsync(CategoryUpdateDto dto)
        {
            if (dto.Id == dto.ParentCategoryId)
                return Result.Failure("You cannot assign a category as its own parent.");

            var category = await _categoryRepository.GetByIdAsync(dto.Id);
            if (category == null)
                return Result.Failure("Category not found.");

            var nameExists = await _categoryRepository.GetCategoryByNameAsync(dto.Name);
            if (nameExists != null && nameExists.Id != category.Id)
                return Result.Failure("name is already exists.");

            var slugExists = await _categoryRepository.GetCategoryBySlugAsync(dto.Slug);
            if (slugExists != null && slugExists.Id != category.Id)
                return Result.Failure("Slug already exists.");

            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl ?? category.ImageUrl; // retain if not changed
            category.ParentCategoryId = dto.ParentCategoryId;

            await _categoryRepository.UpdateAsync(category);
            var result = await _categoryRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to Create category.") : Result.Success();
        }


        public async Task<CategoryUpdateDto> GetCategoryToEditByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return null;

            return category.Adapt<CategoryUpdateDto>();
        }

        public async Task<CategoryDto> GetCategoryDetailsAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id,
                c => c.SubCategories,
                c => c.Products,
                c => c.ParentCategory);

            if (category == null)
                return null;

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                ParentCategory = category.ParentCategory != null
                    ? new CategoryDto
                    {
                        Id = category.ParentCategory.Id,
                        Name = category.ParentCategory.Name,
                        Slug = category.ParentCategory.Slug
                    }
                    : null,
                SubCategories = category.SubCategories?.Select(sub => new CategoryDto
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Description = sub.Description,
                    ImageUrl = sub.ImageUrl,
                    SortOrder = sub.SortOrder,
                    Slug = sub.Slug,
                    IsActive = sub.IsActive
                }).ToList() ?? new List<CategoryDto>(),
                Products = category.Products?.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList() ?? new List<ProductDto>()
            };
            return dto;
        }


        public async Task<Result> ToggleCategoryStatusAsync(Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return Result.Failure("Category not found.");

            category.IsActive = !category.IsActive;

            try
            {
                await _categoryRepository.UpdateAsync(category);
                var result = await _categoryRepository.SaveChangesAsync();

                if (result > 0)
                    return Result.Success();
                else
                    return Result.Failure("No changes were made to the database.");
            }
            catch (Exception ex)
            {
                return Result.Failure("An error occurred while updating the category status.");
            }
        }

        public async Task<CategoryDto> GetSubCategoriesByCategoryId(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(
              id,
              category => category.SubCategories);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                ImageUrl = category.ImageUrl,
                SortOrder = category.SortOrder,
                ParentCategoryId = category.ParentCategoryId,
                CreatedAt = category.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = category.UpdatedAt,
                IsActive = category.IsActive,

                SubCategories = category.SubCategories?.Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    Slug = sc.Slug,
                    ImageUrl = sc.ImageUrl,
                    SortOrder = sc.SortOrder,
                    ParentCategoryId = sc.ParentCategoryId,
                    CreatedAt = sc.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = sc.UpdatedAt,
                    IsActive = sc.IsActive
                }).ToList() ?? new List<CategoryDto>()
            };

            return categoryDto;
        }
    }
}
