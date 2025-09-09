using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Main;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Main;
using Models.People;
using System.Linq;
using System.Linq.Expressions;

namespace Adidas.Application.Services.Main
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICouponService _couponService;
        private readonly ILogger<ProductService> _logger;
        private readonly IUserProductViewRepository _userProductViewRepository;
         
        public ProductService(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            ICouponService couponService,
            ILogger<ProductService> logger,
            IUserProductViewRepository userProductViewRepository)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _couponService = couponService;
            _logger = logger;
            _userProductViewRepository = userProductViewRepository;
        }

        #region Mapping Methods

        // ProductService - ISSUE FOUND: The variant mapping in MapToProductDto is creating new DTOs instead of using proper mapping
        public ProductDto MapToProductDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ShortDescription = p.ShortDescription,
                Price = p.Price,
                SalePrice = p.SalePrice,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                GenderTarget = p.GenderTarget,
                MetaTitle = p.MetaDescription,
                MetaDescription = p.MetaDescription,
                UpdatedAt = p.UpdatedAt,
                CreatedAt = p.CreatedAt ?? new DateTime(),
                IsActive = p.IsActive,
                CategoryName = p.Category?.Name,
                BrandName = p.Brand?.Name,
                
                Category = p.Category != null ? new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name
                } : null,

                Images = p.Variants?.Select(i => new ProductImageDto
                {
                    ProductId = p.Id,
                    VariantId = i.Id,
                    ImageUrl = i.ImageUrl,
                    //AltText = i.AltText

                }).ToList() ?? new List<ProductImageDto>(),

                // FIX: This was the main issue - the variant mapping was incomplete and missing many properties
                Variants = p.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId, // This was missing
                    Sku = v.Sku, // This was missing
                    Color = v.Color,
                    Size = v.Size,
                    ImageUrl = v.ImageUrl,
                    StockQuantity = v.StockQuantity,
                    PriceAdjustment = v.PriceAdjustment,
                    ColorHex = v.Color, // This was missing
                    CreatedAt = v.CreatedAt ?? DateTime.MinValue, // This was missing
                    IsActive = v.IsActive, // This was missing
                    
                    Images = v.Images?.Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        AltText = img.AltText
                    }).ToList() ?? new List<ProductImageDto>()
                }).ToList() ?? new List<ProductVariantDto>(),

                Reviews = p.Reviews?.Select(r => new Review
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    CreatedAt = r.CreatedAt ?? new DateTime(),
                    UpdatedAt = r.UpdatedAt,
                    IsApproved = r.IsApproved,
                    IsActive = r.IsActive,
                    Title = r.Title,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    ReviewText = r.ReviewText
                }).ToList() ?? new List<Review>(),

                InStock = p.Variants?.Any(v => v.StockQuantity > 0) ?? false
            };
        }

        private Product MapToProduct(ProductCreateDto dto)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                Sku = dto.Sku,
                Price = dto.Price,
                SalePrice = dto.SalePrice,
                GenderTarget = dto.GenderTarget,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true // Add this line
            };
        }

        private void MapUpdateDtoToProduct(ProductUpdateDto updateDto, Product product)
        {
            if (updateDto.Name != null)
                product.Name = updateDto.Name;

            if (updateDto.Description != null)
                product.Description = updateDto.Description;

            if (updateDto.ShortDescription != null)
                product.ShortDescription = updateDto.ShortDescription;

            if (updateDto.Price.HasValue)
                product.Price = updateDto.Price.Value;

            if (updateDto.SalePrice.HasValue)
                product.SalePrice = updateDto.SalePrice.Value;

            if (updateDto.GenderTarget.HasValue)
                product.GenderTarget = updateDto.GenderTarget.Value;

            if (updateDto.CategoryId.HasValue)
                product.CategoryId = updateDto.CategoryId.Value;

            if (updateDto.BrandId.HasValue)
                product.BrandId = updateDto.BrandId.Value;

            if (updateDto.MetaTitle != null)
                product.MetaTitle = updateDto.MetaTitle;

            if (updateDto.MetaDescription != null)
                product.MetaDescription = updateDto.MetaDescription;

            product.UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Generic CRUD Operations

        public virtual async Task<OperationResult<ProductDto>> GetByIdAsync(Guid id, params Expression<Func<Product, object>>[] includes)
        {
            try
            {
                IQueryable<Product> query = _productRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Variants)
                    .Include(p => p.Images)
                    .Include(p => p.Reviews)
                    .Where(p => p.Id == id);

                // Apply additional includes if provided
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var entity = await query.FirstOrDefaultAsync();

                if (entity == null)
                    return OperationResult<ProductDto>.Fail("Product not found");

                return OperationResult<ProductDto>.Success(MapToProductDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id {Id} with includes", id);
                return OperationResult<ProductDto>.Fail("Error getting product by id: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductDto>>> GetAllAsync(Func<IQueryable<Product>, IQueryable<Product>>? queryFunc = null)
        {
            try
            {
                IQueryable<Product> query = _productRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Variants)
                    .Include(p => p.Images)
                    .Include(p => p.Reviews);

                if (queryFunc != null)
                {
                    query = queryFunc(query);
                }

                var entities = await query.ToListAsync();
                return OperationResult<IEnumerable<ProductDto>>.Success(entities.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return OperationResult<IEnumerable<ProductDto>>.Fail("Error getting all products: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductDto>>> FindAsync(Func<IQueryable<Product>, IQueryable<Product>> queryFunc)
        {
            try
            {
                IQueryable<Product> query = _productRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Variants)
                    .Include(p => p.Images)
                    .Include(p => p.Reviews);

                query = queryFunc(query);
                var entities = await query.ToListAsync();
                return OperationResult<IEnumerable<ProductDto>>.Success(entities.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding products with predicate");
                return OperationResult<IEnumerable<ProductDto>>.Fail("Error finding products with predicate: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<PagedResultDto<ProductDto>>> GetPagedAsync(int pageNumber, int pageSize, Func<IQueryable<Product>, IQueryable<Product>>? queryFunc = null)
        {
            try
            {
                var pagedResult = await _productRepository.GetPagedAsync(pageNumber, pageSize, queryFunc);

                var mappedResult = new PagedResultDto<ProductDto>
                {
                    Items = pagedResult.Items.Select(MapToProductDto).ToList(),
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize,
                    TotalPages = pagedResult.TotalPages
                };

                return OperationResult<PagedResultDto<ProductDto>>.Success(mappedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged products");
                return OperationResult<PagedResultDto<ProductDto>>.Fail("Error getting paged products: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<int>> CountAsync(Expression<Func<Product, bool>>? predicate = null)
        {
            try
            {
                var count = await _productRepository.CountAsync(predicate);
                return OperationResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting products with predicate");
                return OperationResult<int>.Fail("Error counting products with predicate: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<bool>> ExistsAsync(Expression<Func<Product, bool>> predicate)
        {
            try
            {
                var exists = await _productRepository.ExistsAsync(predicate);
                return OperationResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product existence");
                return OperationResult<bool>.Fail("Error checking product existence: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<ProductDto>> CreateAsync(ProductCreateDto createDto)
        {
            try
            {
                // Check if product name already exists
                bool nameExists = await _productRepository.ExistsAsync(p => p.Name == createDto.Name);
                if (nameExists)
                    return OperationResult<ProductDto>.Fail("Error creating product: Proudct with this name already exists");

                // Domain-specific validation
                await ValidateCreateAsync(createDto);

                // Map DTO to entity
                var entity = MapToProduct(createDto);

                // Pre-create hook
                await BeforeCreateAsync(entity);

                // Add entity
                var createdEntityEntry = await _productRepository.AddAsync(entity);
                await _productRepository.SaveChangesAsync();

                var createdEntity = createdEntityEntry.Entity;

                // Post-create hook
                await AfterCreateAsync(createdEntity);

                // Map to DTO
                var productDto = MapToProductDto(createdEntity);

                return OperationResult<ProductDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return OperationResult<ProductDto>.Fail("Error creating product: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductDto>>> CreateRangeAsync(IEnumerable<ProductCreateDto> createDtos)
        {
            try
            {
                var createDtoList = createDtos.ToList();
                foreach (var createDto in createDtoList)
                {
                    await ValidateCreateAsync(createDto);
                }

                var entities = createDtoList.Select(MapToProduct).ToList();

                foreach (var entity in entities)
                {
                    await BeforeCreateAsync(entity);
                }

                var createdEntityEntries = await _productRepository.AddRangeAsync(entities);
                await _productRepository.SaveChangesAsync();

                var createdEntities = createdEntityEntries.Select(entry => entry.Entity).ToList();

                foreach (var createdEntity in createdEntities)
                {
                    await AfterCreateAsync(createdEntity);
                }

                return OperationResult<IEnumerable<ProductDto>>.Success(createdEntities.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating products");
                return OperationResult<IEnumerable<ProductDto>>.Fail("Error creating products: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<ProductDto>> UpdateAsync(ProductUpdateDto updateDto)
        {
            try
            {
                var existingEntity = await _productRepository.GetByIdAsync(updateDto.Id);
                if (existingEntity == null)
                    return OperationResult<ProductDto>.Fail($"Product with id {updateDto.Id} not found");
                var countName = await _productRepository.CountAsync(p => p.Name == updateDto.Name);
                     
                if (countName==1 && existingEntity.Name != updateDto.Name)
                    return OperationResult<ProductDto>.Fail("Error Updating product: Proudct with this name already exists");

                await ValidateUpdateAsync(updateDto.Id, updateDto);

                // Validate sale price logic
                if (updateDto.SalePrice.HasValue && updateDto.Price.HasValue && updateDto.SalePrice > updateDto.Price)
                {
                    return OperationResult<ProductDto>.Fail("Sale Price cannot be greater than the original Price.");
                }
                else if (updateDto.SalePrice.HasValue && !updateDto.Price.HasValue && existingEntity.Price < updateDto.SalePrice)
                {
                    return OperationResult<ProductDto>.Fail("Sale Price cannot be greater than the original Price.");
                }

                MapUpdateDtoToProduct(updateDto, existingEntity);
                await BeforeUpdateAsync(existingEntity);

                var updatedEntityEntry = await _productRepository.UpdateAsync(existingEntity);
                await _productRepository.SaveChangesAsync();

                var updatedEntity = updatedEntityEntry.Entity;
                await AfterUpdateAsync(updatedEntity);

                return OperationResult<ProductDto>.Success(MapToProductDto(updatedEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with id {Id}", updateDto.Id);
                return OperationResult<ProductDto>.Fail("Error updating product: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<IEnumerable<ProductDto>>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, ProductUpdateDto>> updates)
        {
            try
            {
                var updateList = updates.ToList();
                var entities = new List<Product>();

                foreach (var update in updateList)
                {
                    var existingEntity = await _productRepository.GetByIdAsync(update.Key);
                    if (existingEntity == null)
                        throw new KeyNotFoundException($"Product with id {update.Key} not found");

                    await ValidateUpdateAsync(update.Key, update.Value);
                    MapUpdateDtoToProduct(update.Value, existingEntity);
                    await BeforeUpdateAsync(existingEntity);
                    entities.Add(existingEntity);
                }

                var updatedEntityEntries = await _productRepository.UpdateRangeAsync(entities);
                await _productRepository.SaveChangesAsync();

                var updatedEntities = updatedEntityEntries.Select(entry => entry.Entity).ToList();

                foreach (var updatedEntity in updatedEntities)
                {
                    await AfterUpdateAsync(updatedEntity);
                }

                return OperationResult<IEnumerable<ProductDto>>.Success(updatedEntities.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating products");
                return OperationResult<IEnumerable<ProductDto>>.Fail("Error updating products: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<Product>> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _productRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<Product>.Fail("Product not found");

                await BeforeDeleteAsync(entity);
                var result = await _productRepository.SoftDeleteAsync(id);
                await _productRepository.SaveChangesAsync();
                await AfterDeleteAsync(entity);

                return OperationResult<Product>.Success(result.Entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with id {Id}", id);
                return OperationResult<Product>.Fail("Error deleting product: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<Product>> DeleteAsync(Product entity)
        {
            try
            {
                await BeforeDeleteAsync(entity);
                var result = await _productRepository.SoftDeleteAsync(entity.Id);
                await _productRepository.SaveChangesAsync();
                await AfterDeleteAsync(entity);

                return OperationResult<Product>.Success(result.Entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return OperationResult<Product>.Fail("Error deleting product: " + ex.Message);
            }
        }

        public virtual async Task<OperationResult<IEnumerable<Product>>> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            try
            {
                var entities = new List<Product>();
                foreach (var id in ids)
                {
                    var entity = await _productRepository.GetByIdAsync(id);
                    if (entity != null)
                    {
                        await BeforeDeleteAsync(entity);
                        entities.Add(entity);
                    }
                }

                var result = _productRepository.SoftDeleteRange(entities);
                await _productRepository.SaveChangesAsync();

                foreach (var entity in entities)
                {
                    await AfterDeleteAsync(entity);
                }

                return OperationResult<IEnumerable<Product>>.Success(result.Select(x => x.Entity).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting products");
                return OperationResult<IEnumerable<Product>>.Fail("Error deleting products: " + ex.Message);
            }
        }

        #endregion

        #region Product-Specific Methods

        public async Task<ProductVariantDto?> GetVariantByIdAsync(Guid id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return null;

            return new ProductVariantDto
            {
                Id = variant.Id,
                Color = variant.Color,
                Size = variant.Size,
                StockQuantity = variant.StockQuantity,
                PriceAdjustment = variant.PriceAdjustment,
            };
        }
        public async Task<ProductDto?> GetProductWithVariantsAsync(Guid productId, string? userId)
        {

            var product = await _productRepository.GetProductWithVariantsAsync(productId);
            if (userId != null)
            {
                var alreadyViewed = await _userProductViewRepository
                    .ExistsAsync(userId, productId);

                if (!alreadyViewed)
                {
                    var userProductView = new UserProductView
                    {
                        UserId = userId,
                        ProductId = productId,
                        ViewedAt = DateTime.UtcNow
                    };
                    await _userProductViewRepository.AddAsync(userProductView);
                    await _productRepository.SaveChangesAsync();
                }
            }


            if (product == null) return null;
            return MapToProductDto(product);
        }

        // get others bught this product service 

        public async Task<ProductDto?> GetProductWithVariantsAsync(Guid productId)
        {
            var product = await _productRepository.GetProductWithVariantsAsync(productId);
            if (product == null) return null;
            return MapToProductDto(product);
        }

        public async Task DeleteVariantAsync(Guid id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return;

            _variantRepository.Remove(variant);
            await _variantRepository.SaveChangesAsync();
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsFilteredByCategoryBrandGenderAsync(ProductFilterDto filters)
        {
            var (products, totalCount) = await _productRepository.GetFilteredProductsAsync(filters);

            var productDtos = products.Select(MapToProductDto).ToList();

            return new PagedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(Guid categoryId)
        {
            try
            {
                var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByBrandAsync(Guid brandId)
        {
            try
            {
                var products = await _productRepository.GetProductsByBrandAsync(brandId);
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by brand");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByGenderAsync(Gender gender)
        {
            try
            {
                var products = await _productRepository.GetProductsByGenderAsync(gender);
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by gender");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetFeaturedProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetFeaturedProductsAsync();
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetProductsOnSaleAsync()
        {
            try
            {
                var products = await _productRepository.GetProductsOnSaleAsync();
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products on sale");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ProductDto>> GetProductBySkuAsync(string sku)
        {
            try
            {
                var product = await _productRepository.GetProductBySkuAsync(sku);
                return product == null
                    ? OperationResult<ProductDto>.Fail("Product not found")
                    : OperationResult<ProductDto>.Success(MapToProductDto(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by sku");
                return OperationResult<ProductDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var products = await _productRepository.SearchProductsAsync(searchTerm);
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Select(MapToProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<PagedResultDto<ProductDto>>> GetProductsWithFiltersAsync(ProductFilterDto filters)
        {
            try
            {
                var (products, totalCount) = await _productRepository.GetProductsWithFiltersAsync(
                    filters.PageNumber, filters.PageSize, filters.CategoryId, filters.BrandId,
                    filters.Gender, filters.MinPrice, filters.MaxPrice, filters.SearchTerm);

                var pagedResultDto = new PagedResultDto<ProductDto>
                {
                    Items = products.Select(MapToProductDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filters.PageNumber,
                    PageSize = filters.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
                };

                return OperationResult<PagedResultDto<ProductDto>>.Success(pagedResultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products with filters");
                return OperationResult<PagedResultDto<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> UpdateInventoryAsync(Guid productId, Dictionary<Guid, int> variantStockUpdates)
        {
            try
            {
                foreach (var update in variantStockUpdates)
                {
                    await _variantRepository.UpdateStockAsync(update.Key, update.Value);
                }
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory for product {ProductId}", productId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<decimal>> CalculateDiscountedPriceAsync(Guid productId, string? discountCode = null)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return OperationResult<decimal>.Fail("Product not found");
                }

                var basePrice = product.SalePrice ?? product.Price;

                if (string.IsNullOrEmpty(discountCode))
                {
                    return OperationResult<decimal>.Success(basePrice);
                }

                var discountAmount = await _couponService.CalculateCouponAmountAsync(discountCode, basePrice);
                return OperationResult<decimal>.Success(discountAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discounted price for product {ProductId}", productId);
                return OperationResult<decimal>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<List<ProductDto>>> GetLastAddedProducts()
        {
            try
            {
                var products = await _productRepository.GetAllAsync(
                    p => p.Category,
                    p => p.Images,
                    p => p.Variants,
                    p => p.Reviews
                );

                var lastProducts = products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(MapToProductDto)
                    .ToList();

                if (!lastProducts.Any())
                    return OperationResult<List<ProductDto>>.Fail("No products found.");

                return OperationResult<List<ProductDto>>.Success(lastProducts);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ProductDto>>.Fail($"Error fetching products: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<ProductDto>>> GetSalesProducts()
        {
            try
            {
                var products = await _productRepository.GetAllAsync(
                    p => p.Category,
                    p => p.Images,
                    p => p.Variants,
                    p => p.Reviews
                );

                var salesProducts = products
                    .Where(p => p.SalePrice > 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(MapToProductDto)
                    .ToList();

                if (!salesProducts.Any())
                    return OperationResult<List<ProductDto>>.Fail("No products found.");

                return OperationResult<List<ProductDto>>.Success(salesProducts);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ProductDto>>.Fail($"Error fetching products: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<string> GenerateSkuAsync(string productName)
        {
            var baseSku = productName.Replace(" ", "").ToUpper().Substring(0, Math.Min(productName.Length, 6));
            var counter = 1;
            var sku = $"{baseSku}{counter:D3}";

            while (await _productRepository.GetProductBySkuAsync(sku) != null)
            {
                counter++;
                sku = $"{baseSku}{counter:D3}";
            }

            return sku;
        }

        #endregion

        #region Virtual Methods for Customization

        public virtual Task ValidateCreateAsync(ProductCreateDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("Product name is required");

            if (createDto.Price <= 0)
                throw new ArgumentException("Product price must be greater than 0");

            return Task.CompletedTask;
        }

        public virtual Task ValidateUpdateAsync(Guid id, ProductUpdateDto updateDto) => Task.CompletedTask;

        public virtual async Task BeforeCreateAsync(Product entity)
        {
            if (string.IsNullOrEmpty(entity.Sku))
            {
                entity.Sku = await GenerateSkuAsync(entity.Name);
            }
        }

        public virtual Task AfterCreateAsync(Product entity) => Task.CompletedTask;
        public virtual Task BeforeUpdateAsync(Product entity) => Task.CompletedTask;
        public virtual Task AfterUpdateAsync(Product entity) => Task.CompletedTask;
        public virtual Task BeforeDeleteAsync(Product entity) => Task.CompletedTask;
        public virtual Task AfterDeleteAsync(Product entity) => Task.CompletedTask;
        public async Task<OperationResult<IEnumerable<ProductDto>>> GetPreviouslyPurchasedProductsForAllUsersAsync()
        {
            try
            {
                // First, get the product IDs that have been purchased
                var purchasedProductIds = await _productRepository.GetAll()
                    .Where(p => p.Variants.Any(v => v.OrderItems.Any()))
                    .SelectMany(p => p.Variants
                        .SelectMany(v => v.OrderItems
                            .Select(oi => new { ProductId = p.Id, OrderDate = oi.Order.CreatedAt })))
                    .OrderByDescending(x => x.OrderDate)
                    .Take(10)
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToListAsync();

                if (!purchasedProductIds.Any())
                    return OperationResult<IEnumerable<ProductDto>>.Fail("No purchased products found.");

                // Then fetch the complete product data with all includes
                var purchasedProducts = await _productRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Images)
                    .Include(p => p.Variants)
                        .ThenInclude(v => v.Images) // Include variant images too
                    .Include(p => p.Reviews)
                    .Where(p => purchasedProductIds.Contains(p.Id))
                    .ToListAsync();

                var productDtos = purchasedProducts.Select(MapToProductDto);
                return OperationResult<IEnumerable<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting previously purchased products");
                return OperationResult<IEnumerable<ProductDto>>.Fail($"Error fetching purchased products: {ex.Message}");
            }
        }



        #endregion
    }
}