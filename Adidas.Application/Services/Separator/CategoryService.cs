using AutoMapper;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Models.Separator;
using Microsoft.Extensions.Logging;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Microsoft.Data.SqlClient;
using System.Data.Entity.Infrastructure;

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

        public async Task<IEnumerable<CategoryDto>> GetFilteredCategoriesAsync(string categoryType, string statusFilter, string searchTerm)
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

        public async Task<Result> CreateAsync(CreateCategoryDto createCategoryDto)
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

        public async Task<Result> UpdateAsync(UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.Id);
            if (category == null)
                return Result.Failure("Category not found.");

            // Check for slug uniqueness
            var slugExists = await _categoryRepository.GetCategoryBySlugAsync(dto.Slug);
            if (slugExists != null && dto.Id != category.Id)
                return Result.Failure("Slug already exists.");

            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl ?? category.ImageUrl; // retain if not changed
            category.SortOrder = dto.SortOrder;
            category.ParentCategoryId = dto.ParentCategoryId;

            await _categoryRepository.UpdateAsync(category);
            var result = await _categoryRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to Create category.") : Result.Success();

        }



        public async Task<UpdateCategoryDto> GetCategoryToEditByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return null;

            var dto = new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                SortOrder = category.SortOrder
            };

            return dto;
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


    }
}


//#region Generic Service Overrides

//protected override async Task ValidateCreateAsync(CreateCategoryDto createDto)
//{
//    // Validate slug uniqueness
//    var slugExists = await ValidateSlugUniquenessAsync(createDto.Slug);
//    if (!slugExists)
//    {
//        throw new InvalidOperationException("Category with this slug already exists");
//    }

//    // Validate parent category if provided
//    if (createDto.ParentCategoryId.HasValue)
//    {
//        var parentCategory = await _categoryRepository.GetByIdAsync(createDto.ParentCategoryId.Value);
//        if (parentCategory == null || parentCategory.IsDeleted)
//        {
//            throw new InvalidOperationException("Parent category not found");
//        }
//    }
//}

//protected override async Task ValidateUpdateAsync(Guid id, UpdateCategoryDto updateDto)
//{
//    // Validate slug uniqueness
//    var slugExists = await ValidateSlugUniquenessAsync(updateDto.Slug, id);
//    if (!slugExists)
//    {
//        throw new InvalidOperationException("Another category with this slug already exists");
//    }

//    // Validate parent category if provided
//    if (updateDto.ParentCategoryId.HasValue)
//    {
//        if (updateDto.ParentCategoryId == id)
//        {
//            throw new InvalidOperationException("Category cannot be its own parent");
//        }

//        var parentCategory = await _categoryRepository.GetByIdAsync(updateDto.ParentCategoryId.Value);
//        if (parentCategory == null || parentCategory.IsDeleted)
//        {
//            throw new InvalidOperationException("Parent category not found");
//        }
//    }
//}

//protected override async Task BeforeCreateAsync(Category entity)
//{
//    entity.CreatedAt = DateTime.UtcNow;
//    entity.UpdatedAt = DateTime.UtcNow;
//    entity.IsActive = true;
//    entity.IsDeleted = false;
//}

//protected override async Task BeforeUpdateAsync(Category entity)
//{
//    entity.UpdatedAt = DateTime.UtcNow;
//}

//protected override async Task BeforeDeleteAsync(Category entity)
//{
//    // Check if category has subcategories
//    var subCategories = await _categoryRepository.GetSubCategoriesAsync(entity.Id);
//    if (subCategories.Any())
//    {
//        throw new InvalidOperationException("Cannot delete category that has subcategories");
//    }
//}

//#endregion

//#region Category-Specific Methods

//public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync()
//{
//    try
//    {
//        _logger.LogInformation("Getting main categories");
//        var categories = await _categoryRepository.GetMainCategoriesAsync();
//        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving main categories");
//        throw;
//    }
//}

//public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentCategoryId)
//{
//    try
//    {
//        _logger.LogInformation("Getting subcategories for parent ID: {ParentCategoryId}", parentCategoryId);
//        var categories = await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
//        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving subcategories for parent ID: {ParentCategoryId}", parentCategoryId);
//        throw;
//    }
//}

//public async Task<CategoryResponseDto?> GetCategoryBySlugAsync(string slug)
//{
//    try
//    {
//        _logger.LogInformation("Getting category by slug: {CategorySlug}", slug);
//        var category = await _categoryRepository.GetCategoryBySlugAsync(slug);

//        if (category == null)
//        {
//            _logger.LogWarning("Category not found with slug: {CategorySlug}", slug);
//            return null;
//        }

//        var categoryResponse = _mapper.Map<CategoryResponseDto>(category);

//        // Get subcategories  
//        var subCategories = await _categoryRepository.GetSubCategoriesAsync(category.Id);
//        //categoryResponse.SubCategories = _mapper.Map<List<CategoryListDto>>(subCategories);

//        return categoryResponse;
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving category with slug: {CategorySlug}", slug);
//        throw;
//    }
//}

//public async Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid categoryId)
//{
//    try
//    {
//        _logger.LogInformation("Getting category hierarchy for ID: {CategoryId}", categoryId);
//        var hierarchy = await _categoryRepository.GetCategoryHierarchyAsync(categoryId);
//        var hierarchyDtos = _mapper.Map<List<CategoryHierarchyDto>>(hierarchy);

//        // Set levels
//        for (int i = 0; i < hierarchyDtos.Count; i++)
//        {
//            hierarchyDtos[i].Level = i;
//        }

//        return hierarchyDtos;
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving category hierarchy for ID: {CategoryId}", categoryId);
//        throw;
//    }
//}

//public async Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null)
//{
//    try
//    {
//        _logger.LogInformation("Validating slug uniqueness: {Slug}", slug);
//        var existingCategory = await _categoryRepository.GetCategoryBySlugAsync(slug);

//        if (existingCategory == null)
//        {
//            return true; // Slug is unique
//        }

//        if (excludeId.HasValue && existingCategory.Id == excludeId.Value)
//        {
//            return true; // Same category, slug is valid
//        }

//        return false; // Slug already exists
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error validating slug uniqueness: {Slug}", slug);
//        throw;
//    }
//}

//public async Task<PagedResultDto<CategoryDto>> GetPaginatedCategoryListAsync(int pageNumber, int pageSize)
//{
//    try
//    {
//        _logger.LogInformation("Getting paginated category list - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
//        var (categories, totalCount) = await _categoryRepository.GetPagedAsync(pageNumber, pageSize, c => !c.IsDeleted);
//        var categoryList = _mapper.Map<IEnumerable<CategoryDto>>(categories);

//        return new PagedResultDto<CategoryDto>
//        {
//            Items = categoryList,
//            TotalCount = totalCount,
//            PageNumber = pageNumber,
//            PageSize = pageSize,
//            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
//        };
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving paginated category list");
//        throw;
//    }
//}

//#endregion

