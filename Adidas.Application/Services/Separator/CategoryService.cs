using System.Data.Entity;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Models.Separator;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.DTOs.Common_DTOs;
using Microsoft.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.Models.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;

namespace Adidas.Application.Services.Separator
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductService _productService;

        public CategoryService(
            ICategoryRepository categoryRepository
            ,
            IProductService productService
        )
        {
            _categoryRepository = categoryRepository;
            _productService = productService;
        }

        public async Task<OperationResult<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _categoryRepository.GetAllAsync();
                var dtos = MapToCategoryDtos(entities);

                return OperationResult<IEnumerable<CategoryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.Fail("Error getting all entities: " + ex.Message);
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync(c => c.SubCategories, c => c.Products);

            var mainCategories = categories
                .Where(c => c.ParentCategoryId == null && c.IsActive && !c.IsDeleted)
                .ToList();

            return mainCategories.Select(c => MapToCategoryDto(c, includeSubCategories: true)).ToList();
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Trim();
        }

        public async Task<IEnumerable<CategoryDto>> GetFilteredCategoriesAsync(string categoryType, string statusFilter,
            string searchTerm)
        {
            var categories = await _categoryRepository.GetAllAsync(c => c.SubCategories, c => c.Products);

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

            // Include subcategories for main categories
            return categories.Select(c =>
                c.ParentCategoryId == null
                    ? MapToCategoryDto(c, includeSubCategories: true)
                    : MapToCategoryDto(c)
            ).ToList();
        }

        public async Task<Result> CreateAsync(CategoryCreateDto createCategoryDto)
        {
            try
            {
                var category = new Models.Separator.Category
                {
                    ParentCategoryId = createCategoryDto.ParentCategoryId,
                    Name = createCategoryDto.Name,
                    Slug = createCategoryDto.Slug,
                    SortOrder = createCategoryDto.SortOrder,
                    Description = createCategoryDto.Description,
                    ImageUrl = createCategoryDto.ImageUrl,
                };
                category.Slug = GenerateSlug(createCategoryDto.Name);

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
            category.ImageUrl = dto.ImageUrl ?? category.ImageUrl;
            category.ParentCategoryId = dto.ParentCategoryId;

            await _categoryRepository.UpdateAsync(category);
            var result = await _categoryRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to update category.") : Result.Success();
        }

        public async Task<CategoryUpdateDto> GetCategoryToEditByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return null;

            return MapToCategoryUpdateDto(category);
        }

        public async Task<CategoryDto> GetCategoryDetailsAsync(Guid id)
        {
            // check if the category is main category , if its make its products = all products of its subcategories

            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null) return null;
            if (category.ParentCategoryId == null)
            {
                var subCategories = await _categoryRepository.GetSubCategoriesAsync(category.Id);
                var allProducts = subCategories.SelectMany(sc => sc.Products).ToList();
                category.Products = allProducts;
            }
            return MapToCategoryDto(category, includeRelations: true);
        }

        public async Task<CategoryDto> GetCategoryBySlugAsync(string slug)
        {
            // check if the category is main category , if its make its products = all products of its subcategories
            var category = await _categoryRepository.GetCategoryBySlugAsync(slug);
            if (category == null) return null;
            if (category.ParentCategoryId == null)
            {
                var subCategories = await _categoryRepository.GetSubCategoriesAsync(category.Id);
                var allProducts = subCategories.SelectMany(sc => sc.Products).ToList();
                category.Products = allProducts;
            }
            return MapToCategoryDto(category, includeRelations: true);
       
        }

        public async Task<CategoryDto> GetSubCategoriesByCategorySlug(string slug)
        {
            var category = await _categoryRepository.GetCategoryBySlugAsync(slug);
            return category != null ? MapToCategoryDto(category) : null;
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

            return category != null ? MapToCategoryDto(category, includeSubCategories: true) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetMainCategoriesByType(string Type)
        {
            var categories = await _categoryRepository.GetAllAsync(c => c.SubCategories, c => c.Products);

            categories = categories.Where(c => c.ParentCategoryId == null && c.IsActive && !c.IsDeleted)
               .ToList();

            if (categories != null)
            {
                if (Enum.TryParse<CategoryType>(Type, ignoreCase: true, out var parsedType))
                {
                    categories = categories
                        .Where(c => c.ParentCategoryId == null
                                    && c.IsActive
                                    && c.Type == parsedType)
                        .ToList();
                }
                else
                {
                    categories = new List<Models.Separator.Category>();
                }
            }

            return categories.Select(c => MapToCategoryDto(c, includeSubCategories: true)).ToList();
        }

        // Manual mapping methods
        private IEnumerable<CategoryDto> MapToCategoryDtos(IEnumerable<Category> categories)
        {
            return categories.Select(c => MapToCategoryDto(c)).ToList();
        }

        private CategoryDto MapToCategoryDto(Category category, bool includeRelations = false, bool includeSubCategories = false)
        {
            if (category == null) return null;

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name ?? string.Empty,
                Description = category.Description ?? string.Empty,
                Slug = category.Slug ?? string.Empty,
                ImageUrl = category.ImageUrl,
                SortOrder = category.SortOrder,
                ParentCategoryId = category.ParentCategoryId,
                CreatedAt = category.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = category.UpdatedAt,
                IsActive = category.IsActive,
                Type = category.Type?.ToString() ?? string.Empty,
                HasSubCategories = category.SubCategories?.Any() == true,
                ProductsCount = category.Products?.Count ?? 0
            };

            // Map products if available
            if (category.Products != null)
            {
                dto.Products = category.Products.Select(p => _productService.MapToProductDto(p)).ToList();
            }

            // Map subcategories if requested or if relations should be included
            if ((includeSubCategories || includeRelations) && category.SubCategories != null)
            {
                dto.SubCategories = category.SubCategories.Select(sc => MapToCategoryDto(sc)).ToList();
            }

            // Map parent category if available and relations should be included
            if (includeRelations && category.ParentCategory != null)
            {
                dto.ParentCategory = new CategoryDto
                {
                    Id = category.ParentCategory.Id,
                    Name = category.ParentCategory.Name ?? string.Empty,
                    Slug = category.ParentCategory.Slug ?? string.Empty,
                    Description = category.ParentCategory.Description ?? string.Empty
                };
            }

            return dto;
        }

        private CategoryUpdateDto MapToCategoryUpdateDto(Category category)
        {
            if (category == null) return null;

            return new CategoryUpdateDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
            };
        }

        //private ProductDto MapToProductDto(Product product)
        //{
        //    if (product == null) return null;

        //    return new ProductDto
        //    {
        //        Id = product.Id,
        //        Name = product.Name ?? string.Empty,
        //        Description = product.Description,
        //        Price = product.Price,
        //        IsActive = product.IsActive,
        //        CategoryId = product.CategoryId,
        //        BrandId = product.BrandId,
        //        CreatedAt = product.CreatedAt ?? DateTime.MinValue,
        //        UpdatedAt = product.UpdatedAt,
        //        InStock = product.Variants != null && product.Variants.Any(v => v.StockQuantity > 0),
        //        SalePrice = product.SalePrice,
        //        GenderTarget = product.GenderTarget,
        //        ShortDescription = product.ShortDescription ?? string.Empty,
        //        Sku = product.Sku ?? string.Empty,
        //        MetaTitle = product.MetaTitle,
        //        MetaDescription = product.MetaDescription,
        //        CategoryName = product.Category?.Name ?? "No Category",
        //        BrandName = product.Brand?.Name ?? "No Brand"
             

        //    };
        //}
    }
}