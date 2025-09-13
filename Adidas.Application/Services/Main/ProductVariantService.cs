using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using iTextSharp.text.log;

namespace Adidas.Application.Services.Main
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly ILogger<ProductVariantService> _logger;
        private readonly Cloudinary _cloudinary;

        // Optional: set to a Cloudinary placeholder URL if you want a default
        private const string DefaultImageUrl = null; // e.g., "https://res.cloudinary.com/<cloud>/image/upload/v.../placeholder.png";

        public ProductVariantService(
            Cloudinary cloudinary,
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            ILogger<ProductVariantService> logger)
        {
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;
            _logger = logger;
            _cloudinary = cloudinary;
        }

        #region Manual Mapping Methods

        private ProductVariantDto MapToProductVariantDto(ProductVariant variant)
        {
            if (variant == null) return null;

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku ?? string.Empty,
                Color = variant.Color ?? string.Empty,
                Size = variant.Size ?? string.Empty,
                StockQuantity = variant.StockQuantity,
                PriceAdjustment = variant.PriceAdjustment,
                ColorHex = variant.Color,            // ✅ fix
                CreatedAt = variant.CreatedAt ?? DateTime.MinValue,
                IsActive = variant.IsActive,
                ImageUrl = variant.ImageUrl,
                Product = null,
                Images = new List<ProductImageDto>()
            };
        }

        private ProductVariant MapToProductVariant(ProductVariantCreateDto createDto)
        {
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            return new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = createDto.ProductId,
                Color = createDto.Color ?? string.Empty,
                Size = createDto.Size ?? string.Empty,
                StockQuantity = createDto.StockQuantity,
                PriceAdjustment = createDto.PriceAdjustment,
                Sku = string.Empty, // will be generated later
                ImageUrl = createDto.ImageUrl ?? DefaultImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                AddedById = null,
                 
            };
        }


        private void MapUpdateDtoToProductVariant(ProductVariantUpdateDto updateDto, ProductVariant variant)
        {
            if (updateDto == null || variant == null)
                throw new ArgumentNullException(updateDto == null ? nameof(updateDto) : nameof(variant));

            if (updateDto.ProductId != Guid.Empty)
                variant.ProductId = updateDto.ProductId;

            // Use ColorHex if provided, otherwise use Color
            if (!string.IsNullOrWhiteSpace(updateDto.ColorHex))
                variant.Color = updateDto.ColorHex; // Store hex in Color field
            else if (!string.IsNullOrWhiteSpace(updateDto.Color))
                variant.Color = updateDto.Color;

            if (!string.IsNullOrWhiteSpace(updateDto.Size))
                variant.Size = updateDto.Size;

            if (updateDto.StockQuantity >= 0)
                variant.StockQuantity = updateDto.StockQuantity;

            variant.PriceAdjustment = updateDto.PriceAdjustment;

            if (updateDto.IsActive.HasValue)
                variant.IsActive = updateDto.IsActive.Value;

            variant.UpdatedAt = DateTime.UtcNow;
        }


        #endregion

        #region Generic CRUD Operations

        public virtual async Task<OperationResult<ProductVariantDto>> GetByIdAsync(Guid id, params Expression<Func<ProductVariant, object>>[] includes)
        {
            try
            {
                if (id == Guid.Empty)
                    return OperationResult<ProductVariantDto>.Fail("Invalid product variant ID");

                IQueryable<ProductVariant> query = _productVariantRepository.GetAll()
                    .Include(v => v.Product)
                    .Include(v => v.Images)
                    .Where(v => v.Id == id && !v.IsDeleted);

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var entity = await query.FirstOrDefaultAsync();

                if (entity == null)
                    return OperationResult<ProductVariantDto>.Fail("Product variant not found");

                return OperationResult<ProductVariantDto>.Success(MapToProductVariantDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product variant by id {Id}", id);
                return OperationResult<ProductVariantDto>.Fail($"Error getting product variant by id: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductVariantDto>>> GetAllAsync(Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>>? queryFunc = null)
        {
            try
            {
                IQueryable<ProductVariant> query = _productVariantRepository.GetAll()
                    .Include(v => v.Product)
                    .Include(v => v.Images)
                    .Where(v => !v.IsDeleted);

                if (queryFunc != null)
                {
                    query = queryFunc(query);
                }

                var entities = await query.ToListAsync();
                var mappedEntities = entities.Select(MapToProductVariantDto).Where(dto => dto != null);
                return OperationResult<IEnumerable<ProductVariantDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all product variants");
                return OperationResult<IEnumerable<ProductVariantDto>>.Fail($"Error getting all product variants: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductVariantDto>>> FindAsync(Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>> queryFunc)
        {
            try
            {
                if (queryFunc == null)
                    throw new ArgumentNullException(nameof(queryFunc));

                IQueryable<ProductVariant> query = _productVariantRepository.GetAll()
                    .Include(v => v.Product)
                    .Include(v => v.Images)
                    .Where(v => !v.IsDeleted);

                query = queryFunc(query);
                var entities = await query.ToListAsync();
                var mappedEntities = entities.Select(MapToProductVariantDto).Where(dto => dto != null);
                return OperationResult<IEnumerable<ProductVariantDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding product variants with predicate");
                return OperationResult<IEnumerable<ProductVariantDto>>.Fail($"Error finding product variants with predicate: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<PagedResultDto<ProductVariantDto>>> GetPagedAsync(int pageNumber, int pageSize, Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>>? queryFunc = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>> combinedQueryFunc = (query) =>
                {
                    query = query.Where(v => !v.IsDeleted);
                    if (queryFunc != null)
                        query = queryFunc(query);
                    return query;
                };

                var pagedResult = await _productVariantRepository.GetPagedAsync(pageNumber, pageSize, combinedQueryFunc);

                var mappedItems = pagedResult.Items.Select(MapToProductVariantDto).Where(dto => dto != null).ToList();

                var mappedResult = new PagedResultDto<ProductVariantDto>
                {
                    Items = mappedItems,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize,
                    TotalPages = pagedResult.TotalPages
                };

                return OperationResult<PagedResultDto<ProductVariantDto>>.Success(mappedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged product variants");
                return OperationResult<PagedResultDto<ProductVariantDto>>.Fail($"Error getting paged product variants: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<int>> CountAsync(Expression<Func<ProductVariant, bool>>? predicate = null)
        {
            try
            {
                Expression<Func<ProductVariant, bool>> combinedPredicate;
                if (predicate == null)
                {
                    combinedPredicate = v => !v.IsDeleted;
                }
                else
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(typeof(ProductVariant), "v");
                    var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ProductVariant.IsDeleted));
                    var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);

                    var predicateBody = System.Linq.Expressions.Expression.Invoke(predicate, parameter);
                    var combinedBody = System.Linq.Expressions.Expression.AndAlso(notDeleted, predicateBody);
                    combinedPredicate = System.Linq.Expressions.Expression.Lambda<Func<ProductVariant, bool>>(combinedBody, parameter);
                }

                var count = await _productVariantRepository.CountAsync(combinedPredicate);
                return OperationResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting product variants");
                return OperationResult<int>.Fail($"Error counting product variants: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<bool>> ExistsAsync(Expression<Func<ProductVariant, bool>> predicate)
        {
            try
            {
                if (predicate == null)
                    throw new ArgumentNullException(nameof(predicate));

                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(ProductVariant), "v");
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ProductVariant.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);

                var predicateBody = System.Linq.Expressions.Expression.Invoke(predicate, parameter);
                var combinedBody = System.Linq.Expressions.Expression.AndAlso(notDeleted, predicateBody);
                var combinedPredicate = System.Linq.Expressions.Expression.Lambda<Func<ProductVariant, bool>>(combinedBody, parameter);

                var exists = await _productVariantRepository.ExistsAsync(combinedPredicate);
                return OperationResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product variant existence");
                return OperationResult<bool>.Fail($"Error checking product variant existence: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<ProductVariantDto>> CreateAsync(ProductVariantCreateDto createDto)
        {
            try
            {
                if (createDto == null)
                    return OperationResult<ProductVariantDto>.Fail("Create data cannot be null");

                await ValidateCreateAsync(createDto);

                var existingVariants = await _productVariantRepository.GetAll()
                    .Where(v => v.ProductId == createDto.ProductId &&
                                v.Size.ToLower() == createDto.Size.ToLower() &&
                                v.Color.ToLower() == createDto.Color.ToLower() &&
                                !v.IsDeleted)
                    .ToListAsync();

                if (existingVariants.Any())
                    return OperationResult<ProductVariantDto>.Fail("A variant with the same size and color already exists for this product.");

                var entity = MapToProductVariant(createDto);

                if (string.IsNullOrWhiteSpace(entity.Sku))
                    entity.Sku = $"SKU-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
                {
                    var imageUploadResult = await HandleImageUploadAsync(createDto.ImageFile);
                    if (imageUploadResult.IsSuccess)
                    {
                        entity.ImageUrl = imageUploadResult.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload image for product variant: {Error}", imageUploadResult.ErrorMessage);
                        entity.ImageUrl = DefaultImageUrl;
                    }
                }
                else
                {
                    entity.ImageUrl = entity.ImageUrl ?? DefaultImageUrl; // leave null or use placeholder
                }

                await BeforeCreateAsync(entity);

                var createdEntityEntry = await _productVariantRepository.AddAsync(entity);
                await _productVariantRepository.SaveChangesAsync();

                var createdEntity = createdEntityEntry.Entity;
                await AfterCreateAsync(createdEntity);

                return OperationResult<ProductVariantDto>.Success(MapToProductVariantDto(createdEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product variant");
                return OperationResult<ProductVariantDto>.Fail($"Error creating product variant: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductVariantDto>>> CreateRangeAsync(IEnumerable<ProductVariantCreateDto> createDtos)
        {
            try
            {
                if (createDtos == null)
                    return OperationResult<IEnumerable<ProductVariantDto>>.Fail("Create data cannot be null");

                var createDtoList = createDtos.ToList();
                if (!createDtoList.Any())
                    return OperationResult<IEnumerable<ProductVariantDto>>.Success(new List<ProductVariantDto>());

                foreach (var createDto in createDtoList)
                    await ValidateCreateAsync(createDto);

                var entities = new List<ProductVariant>();
                foreach (var createDto in createDtoList)
                {
                    var entity = MapToProductVariant(createDto);
                    if (string.IsNullOrWhiteSpace(entity.Sku))
                        entity.Sku = $"SKU-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                    // Note: bulk create with images typically uploads per item; you can add that here if needed
                    entities.Add(entity);
                }

                foreach (var entity in entities)
                    await BeforeCreateAsync(entity);

                var createdEntityEntries = await _productVariantRepository.AddRangeAsync(entities);
                await _productVariantRepository.SaveChangesAsync();

                var createdEntities = createdEntityEntries.Select(entry => entry.Entity).ToList();

                foreach (var createdEntity in createdEntities)
                    await AfterCreateAsync(createdEntity);

                var mappedEntities = createdEntities.Select(MapToProductVariantDto).Where(dto => dto != null);
                return OperationResult<IEnumerable<ProductVariantDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product variants");
                return OperationResult<IEnumerable<ProductVariantDto>>.Fail($"Error creating product variants: {ex.Message}");
            }
        }


        public virtual async Task<OperationResult<ProductVariantDto>> UpdateAsync(ProductVariantUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null)
                    return OperationResult<ProductVariantDto>.Fail("Update data cannot be null");

                if (updateDto.Id == Guid.Empty)
                    return OperationResult<ProductVariantDto>.Fail("Product variant ID is required");

                var existingEntity = await _productVariantRepository.GetByIdAsync(updateDto.Id);
                if (existingEntity == null || existingEntity.IsDeleted)
                    return OperationResult<ProductVariantDto>.Fail($"Product variant with id {updateDto.Id} not found");

                await ValidateUpdateAsync(updateDto.Id, updateDto);

                var duplicates = await _productVariantRepository.GetAll()
                    .Where(v => v.ProductId == updateDto.ProductId &&
                                v.Size.ToLower() == updateDto.Size.ToLower() &&
                                v.Color.ToLower() == updateDto.Color.ToLower() &&
                                v.Id != updateDto.Id &&
                                !v.IsDeleted)
                    .ToListAsync();

                if (duplicates.Any())
                    return OperationResult<ProductVariantDto>.Fail("A product variant with the same Product, Size, and Color already exists.");

                // Handle image upload first (following CreateAsync pattern)
                string imageUrlToUse = existingEntity.ImageUrl; // Keep existing URL as default

                if (updateDto.ImageFile != null && updateDto.ImageFile.Length > 0)
                {
                    var imageUploadResult = await HandleImageUploadAsync(updateDto.ImageFile);
                    if (imageUploadResult.IsSuccess)
                    {
                        imageUrlToUse = imageUploadResult.Data;
                        _logger.LogInformation("Successfully updated image for product variant {Id}", updateDto.Id);

                        // Store old image URL for cleanup after successful update
                        string oldImageUrl = existingEntity.ImageUrl;

                        // Clean up old image immediately after successful upload (like in CreateAsync)
                        if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != DefaultImageUrl)
                        {
                            await DeleteOldImageAsync(oldImageUrl);
                        }
                    }
                    else
                    {
                        // Following CreateAsync pattern - log warning but continue with existing image
                        _logger.LogWarning("Failed to upload image for product variant {Id}: {Error}", updateDto.Id, imageUploadResult.ErrorMessage);
                        // Keep existing imageUrlToUse (don't change it)
                    }
                }

                // Map other properties (following CreateAsync pattern)
                MapUpdateDtoToProductVariant(updateDto, existingEntity);

                // Set the image URL (either new or existing)
                existingEntity.ImageUrl = imageUrlToUse;

                await BeforeUpdateAsync(existingEntity);

                var updatedEntityEntry = await _productVariantRepository.UpdateAsync(existingEntity);
                await _productVariantRepository.SaveChangesAsync();

                var updatedEntity = updatedEntityEntry.Entity;
                await AfterUpdateAsync(updatedEntity);

                return OperationResult<ProductVariantDto>.Success(MapToProductVariantDto(updatedEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product variant with id {Id}", updateDto?.Id);
                return OperationResult<ProductVariantDto>.Fail($"Error updating product variant: {ex.Message}");
            }
        }
        public virtual async Task<OperationResult<IEnumerable<ProductVariantDto>>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, ProductVariantUpdateDto>> updates)
        {
            try
            {
                if (updates == null)
                    return OperationResult<IEnumerable<ProductVariantDto>>.Fail("Updates cannot be null");

                var updateList = updates.ToList();
                if (!updateList.Any())
                    return OperationResult<IEnumerable<ProductVariantDto>>.Success(new List<ProductVariantDto>());

                var entities = new List<ProductVariant>();

                foreach (var update in updateList)
                {
                    if (update.Key == Guid.Empty)
                        throw new ArgumentException($"Invalid product variant ID: {update.Key}");

                    var existingEntity = await _productVariantRepository.GetByIdAsync(update.Key);
                    if (existingEntity == null || existingEntity.IsDeleted)
                        throw new KeyNotFoundException($"Product variant with id {update.Key} not found");

                    await ValidateUpdateAsync(update.Key, update.Value);

                    // If you support per-item image update in range, handle here

                    MapUpdateDtoToProductVariant(update.Value, existingEntity);
                    await BeforeUpdateAsync(existingEntity);
                    entities.Add(existingEntity);
                }

                var updatedEntityEntries = await _productVariantRepository.UpdateRangeAsync(entities);
                await _productVariantRepository.SaveChangesAsync();

                var updatedEntities = updatedEntityEntries.Select(entry => entry.Entity).ToList();

                foreach (var updatedEntity in updatedEntities)
                    await AfterUpdateAsync(updatedEntity);

                var mappedEntities = updatedEntities.Select(MapToProductVariantDto).Where(dto => dto != null);
                return OperationResult<IEnumerable<ProductVariantDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product variants");
                return OperationResult<IEnumerable<ProductVariantDto>>.Fail($"Error updating product variants: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<ProductVariant>> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return OperationResult<ProductVariant>.Fail("Invalid product variant ID");

                // Load entity with children
                var entity = await _productVariantRepository.GetByIdAsync(
                    id,
                    c => c.OrderItems,
                    c => c.CartItems
                );

                if (entity == null || entity.IsDeleted)
                    return OperationResult<ProductVariant>.Fail("Product variant not found or already deleted");

                // 🔎 Check if it has children
                if ((entity.OrderItems?.Any() ?? false) || (entity.CartItems?.Any() ?? false))
                {
                    return OperationResult<ProductVariant>.Fail("Cannot delete product variant because it has related order items or cart items.");
                }

                await BeforeDeleteAsync(entity);

                await _productVariantRepository.HardDeleteAsync(id);
                await _productVariantRepository.SaveChangesAsync();

                await AfterDeleteAsync(entity);

                return OperationResult<ProductVariant>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product variant with id {Id}", id);
                return OperationResult<ProductVariant>.Fail($"Error deleting product variant: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<ProductVariant>> DeleteAsync(ProductVariant entity)
        {
            try
            {
                if (entity == null)
                    return OperationResult<ProductVariant>.Fail("Entity cannot be null");

                if (entity.IsDeleted)
                    return OperationResult<ProductVariant>.Fail("Product variant is already deleted");

                await BeforeDeleteAsync(entity);
                var result = await _productVariantRepository.SoftDeleteAsync(entity.Id);
                await _productVariantRepository.SaveChangesAsync();
                await AfterDeleteAsync(entity);

                return OperationResult<ProductVariant>.Success(result.Entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product variant");
                return OperationResult<ProductVariant>.Fail($"Error deleting product variant: {ex.Message}");
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductVariant>>> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            try
            {
                if (ids == null)
                    return OperationResult<IEnumerable<ProductVariant>>.Fail("IDs cannot be null");

                var idList = ids.Where(id => id != Guid.Empty).ToList();
                if (!idList.Any())
                    return OperationResult<IEnumerable<ProductVariant>>.Success(new List<ProductVariant>());

                var entities = new List<ProductVariant>();
                foreach (var id in idList)
                {
                    var entity = await _productVariantRepository.GetByIdAsync(id);
                    if (entity != null && !entity.IsDeleted)
                    {
                        await BeforeDeleteAsync(entity);
                        entities.Add(entity);
                    }
                }

                if (!entities.Any())
                    return OperationResult<IEnumerable<ProductVariant>>.Success(new List<ProductVariant>());

                var result = _productVariantRepository.SoftDeleteRange(entities);
                await _productVariantRepository.SaveChangesAsync();

                foreach (var entity in entities)
                    await AfterDeleteAsync(entity);

                return OperationResult<IEnumerable<ProductVariant>>.Success(result.Select(x => x.Entity).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product variants");
                return OperationResult<IEnumerable<ProductVariant>>.Fail($"Error deleting product variants: {ex.Message}");
            }
        }

        #endregion

        #region ProductVariant-Specific Methods

        public async Task<bool> AddImageAsync(Guid variantId, IFormFile imageFile)
        {
            try
            {
                if (variantId == Guid.Empty || imageFile == null || imageFile.Length == 0)
                    return false;

                var variant = await _productVariantRepository.GetByIdAsync(variantId);
                if (variant == null || variant.IsDeleted)
                    return false;

                var imageUploadResult = await HandleImageUploadAsync(imageFile);
                if (!imageUploadResult.IsSuccess)
                    return false;

                var image = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = imageUploadResult.Data,
                    VariantId = variantId,
                    ProductId = variant.ProductId,
                    IsPrimary = false,
                    SortOrder = 0,
                    AltText = $"Image for {variant.Color} {variant.Size}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _productImageRepository.AddAsync(image);
                await _productImageRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to product variant {VariantId}", variantId);
                return false;
            }
        }

        public async Task<ProductVariantDto?> GetBySkuAsync(string sku)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return null;

                var variant = await _productVariantRepository.GetVariantBySkuAsync(sku);
                if (variant == null || variant.IsDeleted)
                    return null;

                return MapToProductVariantDto(variant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product variant by SKU {Sku}", sku);
                return null;
            }
        }

        public async Task<OperationResult<ProductVariantDto>> UpdateAsync(Guid id, ProductVariantUpdateDto productVariantUpdateDto)
        {
            if (productVariantUpdateDto == null)
                return OperationResult<ProductVariantDto>.Fail("Update data cannot be null");

            productVariantUpdateDto.Id = id;
            return await UpdateAsync(productVariantUpdateDto);
        }

        #endregion

        #region Private Helper Methods


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
                    Folder = "product_variants",
                    // Add these for better control
                    Overwrite = false,
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

                _logger.LogInformation("Successfully uploaded image to Cloudinary: {PublicId}", uploadResult.PublicId);
                return OperationResult<string>.Success(uploadResult.SecureUrl.AbsoluteUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
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

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (!string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Cloudinary deletion returned: {Result} for {PublicId}", result.Result, publicId);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted Cloudinary image: {PublicId}", publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting Cloudinary image: {ImageUrl}", imageUrl);
            }
        }

        #endregion

        #region Virtual Methods for Customization

        public virtual Task ValidateCreateAsync(ProductVariantCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (createDto.ProductId == Guid.Empty)
                throw new ArgumentException("Product ID is required", nameof(createDto.ProductId));

            if (string.IsNullOrWhiteSpace(createDto.Color))
                throw new ArgumentException("Color is required", nameof(createDto.Color));

            if (string.IsNullOrWhiteSpace(createDto.Size))
                throw new ArgumentException("Size is required", nameof(createDto.Size));

            if (createDto.StockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(createDto.StockQuantity));

            if (createDto.Color.Length > 50)
                throw new ArgumentException("Color name cannot exceed 50 characters", nameof(createDto.Color));

            if (createDto.Size.Length > 20)
                throw new ArgumentException("Size cannot exceed 20 characters", nameof(createDto.Size));

            return Task.CompletedTask;
        }

        public virtual Task ValidateUpdateAsync(Guid id, ProductVariantUpdateDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            if (id == Guid.Empty)
                throw new ArgumentException("Product variant ID is required", nameof(id));

            if (updateDto.ProductId == Guid.Empty)
                throw new ArgumentException("Product ID is required", nameof(updateDto.ProductId));

            if (string.IsNullOrWhiteSpace(updateDto.Color))
                throw new ArgumentException("Color is required", nameof(updateDto.Color));

            if (string.IsNullOrWhiteSpace(updateDto.Size))
                throw new ArgumentException("Size is required", nameof(updateDto.Size));

            if (updateDto.StockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(updateDto.StockQuantity));

            if (updateDto.Color.Length > 50)
                throw new ArgumentException("Color name cannot exceed 50 characters", nameof(updateDto.Color));

            if (updateDto.Size.Length > 20)
                throw new ArgumentException("Size cannot exceed 20 characters", nameof(updateDto.Size));

            return Task.CompletedTask;
        }

        public virtual Task BeforeCreateAsync(ProductVariant entity) => Task.CompletedTask;
        public virtual Task AfterCreateAsync(ProductVariant entity) => Task.CompletedTask;
        public virtual Task BeforeUpdateAsync(ProductVariant entity) => Task.CompletedTask;
        public virtual Task AfterUpdateAsync(ProductVariant entity) => Task.CompletedTask;
        public virtual Task BeforeDeleteAsync(ProductVariant entity) => Task.CompletedTask;
        public virtual Task AfterDeleteAsync(ProductVariant entity) => Task.CompletedTask;

        #endregion
    }
}

 