using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Models.Separator;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.DTOs.Common_DTOs;
using Microsoft.Data.SqlClient;
using Adidas.DTOs.CommonDTOs;
using Adidas.Application.Contracts.ServicesContracts.Main;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Separator
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductService _productService;
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CategoryService> _logger;

        private const string DefaultImageUrl = null; // e.g., "https://res.cloudinary.com/<cloud>/image/upload/v.../placeholder.png";

        public CategoryService(
            ICategoryRepository categoryRepository,
            IProductService productService,
            Cloudinary cloudinary,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _productService = productService;
            _cloudinary = cloudinary;
            _logger = logger;
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
            var categories = await _categoryRepository.GetAllAsync(c => c.ParentCategory, c => c.Products);

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
                var nameExists = await _categoryRepository.GetCategoryByNameAsync(createCategoryDto.Name);
                if (nameExists != null)
                    return Result.Failure("Name already exists.");

                 
                 
                var category = new Models.Separator.Category
                {
                    ParentCategoryId = createCategoryDto.ParentCategoryId,
                    Name = createCategoryDto.Name,
                    Slug = createCategoryDto.Slug,
                    SortOrder = createCategoryDto.SortOrder,
                    Description = createCategoryDto.Description,
                };
                category.Slug = GenerateSlug(createCategoryDto.Name);

                // Handle image upload if provided
                if (createCategoryDto.ImageFile != null && createCategoryDto.ImageFile.Length > 0)
                {
                    var imageUploadResult = await HandleImageUploadAsync(createCategoryDto.ImageFile);
                    if (imageUploadResult.IsSuccess)
                    {
                        category.ImageUrl = imageUploadResult.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload image for category: {Error}", imageUploadResult.ErrorMessage);
                        category.ImageUrl = DefaultImageUrl;
                    }
                }
                else
                {
                    category.ImageUrl = createCategoryDto.ImageUrl ?? DefaultImageUrl;
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
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

            // Delete the image from Cloudinary before deleting the category
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                await DeleteOldImageAsync(category.ImageUrl);
            }

            await _categoryRepository.HardDeleteAsync(id);
            var result = await _categoryRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to delete category.") : Result.Success();
        }

        public async Task<Result> UpdateAsync(CategoryUpdateDto dto)
        {
            if (dto.Id == dto.ParentCategoryId)
                return Result.Failure("You cannot assign a category as its own parent.");

            var category = await _categoryRepository.GetCategoryByIdAsync(dto.Id);
            if (category == null)
                return Result.Failure("Category not found.");

            var nameExists = await _categoryRepository.GetCategoryByNameAsync(dto.Name);
            if (nameExists != null && nameExists.Id != category.Id)
                return Result.Failure("Name already exists.");

            var slugExists = await _categoryRepository.GetCategoryBySlugAsync(dto.Slug);
            if (slugExists != null && slugExists.Id != category.Id)
                return Result.Failure("Slug already exists.");

             if (dto.ImageFile != null && dto.ImageFile.Length > 0)
             {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                    await DeleteOldImageAsync(category.ImageUrl);

                var imageUploadResult = await HandleImageUploadAsync(dto.ImageFile);
                if (imageUploadResult.IsSuccess)
                {
                    category.ImageUrl = imageUploadResult.Data;
                }
                else
                {
                    return Result.Failure($"Failed to upload image: {imageUploadResult.ErrorMessage}");
                }
            }
            else if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != category.ImageUrl)
            {
                category.ImageUrl = dto.ImageUrl;
            }

            // 🔹 Update other fields
            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.ParentCategoryId = dto.ParentCategoryId;
            //category.IsActive = dto.IsActive;
            //category.SortOrder = dto.SortOrder;

            // 🔹 Save changes
            var rowsAffected = await _categoryRepository.SaveChangesAsync();

            return rowsAffected > 0
                ? Result.Success()
                : Result.Failure("No changes were saved. Entity might be unchanged.");
        }

        public async Task<CategoryUpdateDto> GetCategoryToEditByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

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

        #region Image Handling Methods

        private async Task<OperationResult<string>> HandleImageUploadAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return OperationResult<string>.Fail("Invalid image file");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return OperationResult<string>.Fail("Invalid file type. Only image files are allowed.");

                const long maxFileSize = 5 * 1024 * 1024;
                if (imageFile.Length > maxFileSize)
                    return OperationResult<string>.Fail("File size too large. Maximum size is 5MB.");

                await using var stream = imageFile.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, stream),
                    Folder = "categories",
                    Overwrite = true,   // 🔹 ensures replacement
                    Invalidate = true,  // 🔹 clears CDN cache
                    UniqueFilename = true,
                    UseFilename = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return OperationResult<string>.Fail($"Image upload failed: {uploadResult.Error.Message}");
                }

                if (string.IsNullOrEmpty(uploadResult.SecureUrl?.AbsoluteUri))
                {
                    _logger.LogError("Cloudinary upload returned null or empty URL");
                    return OperationResult<string>.Fail("Image upload failed: No URL returned");
                }

                _logger.LogInformation("Successfully uploaded category image to Cloudinary: {PublicId}", uploadResult.PublicId);
                return OperationResult<string>.Success(uploadResult.SecureUrl.AbsoluteUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading category image to Cloudinary");
                return OperationResult<string>.Fail($"Error uploading image: {ex.Message}");
            }
        }
        private async Task DeleteOldImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                    return;

                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                    return;

                if (!uri.Host.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
                    return;

                var path = uri.AbsolutePath;
                var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
                if (uploadIndex < 0) return;

                var afterUpload = path.Substring(uploadIndex + "/upload/".Length);

                var segments = afterUpload.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (segments.Count > 0 && segments[0].StartsWith("v") && int.TryParse(segments[0].Substring(1), out _))
                {
                    segments.RemoveAt(0);
                }
                if (!segments.Any()) return;

                var publicIdWithExt = string.Join("/", segments);
                var publicId = Path.ChangeExtension(publicIdWithExt, null);

                var deletionParams = new DeletionParams(publicId) { Invalidate = true };
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (!string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Cloudinary deletion returned: {Result} for {PublicId}", result.Result, publicId);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted Cloudinary category image: {PublicId}", publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting Cloudinary category image: {ImageUrl}", imageUrl);
            }
        }

        #endregion

        #region Manual mapping methods
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
                ParentName = category.ParentCategory != null
                        ? category.ParentCategory.Name
                        : string.Empty,
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

        #endregion
    }
}