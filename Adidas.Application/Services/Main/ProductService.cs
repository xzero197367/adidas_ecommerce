using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.Extensions.Logging;
using Models.People;
using System.Linq.Expressions;

namespace Adidas.Application.Services.Main
{
    public class ProductService : GenericService<Product, ProductDto, ProductCreateDto, ProductUpdateDto>,
        IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICouponService _couponService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            ICouponService couponService,
            ILogger<ProductService> logger) : base(productRepository, logger)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _couponService = couponService;
            _logger = logger;
        }
        private ProductDto MapToProductDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                SalePrice = p.SalePrice,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                GenderTarget = p.GenderTarget,
                UpdatedAt = p.UpdatedAt,
                CategoryName = p.Category?.Name,
                BrandName = p.Brand?.Name,

                Variants = p.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Color = v.Color,
                    Size = v.Size,
                    StockQuantity = v.StockQuantity,
                    PriceAdjustment = v.PriceAdjustment
                }).ToList() ?? new List<ProductVariantDto>(),

                InStock = p.Variants?.Any(v => v.StockQuantity > 0) ?? false
            };
        }
        private Product MapToProduct(ProductCreateDto dto)
        {
            return new Product
            {
               
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
                BrandId = dto.BrandId
            };
        }



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

        protected Task ValidateCreateAsync(ProductCreateDto productCreateDto)
        {
            if (string.IsNullOrWhiteSpace(productCreateDto.Name))
                throw new ArgumentException("Product name is required");

            if (productCreateDto.Price <= 0)
                throw new ArgumentException("Product price must be greater than 0");

            return Task.CompletedTask;
        }

        protected async Task BeforeCreateAsync(Product entity)
        {
            if (string.IsNullOrEmpty(entity.Sku))
            {
                entity.Sku = await GenerateSkuAsync(entity.Name);
            }
        }

        public override async Task<OperationResult<ProductDto>> UpdateAsync(ProductUpdateDto updateDto)
        {
            var existingEntity = await _productRepository.GetByIdAsync(updateDto.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Product with id {updateDto.Id} not found");

            if (updateDto.SalePrice.HasValue && updateDto.Price.HasValue && updateDto.SalePrice > updateDto.Price)
            {
                return OperationResult<ProductDto>.Fail("Sale Price cannot be greater than the original Price.");
            }
            else if (updateDto.SalePrice.HasValue && !updateDto.Price.HasValue && existingEntity.Price < updateDto.SalePrice)
            {
                return OperationResult<ProductDto>.Fail("Sale Price cannot be greater than the original Price.");
            }

            if (updateDto.Name != null)
                existingEntity.Name = updateDto.Name;

            if (updateDto.Description != null)
                existingEntity.Description = updateDto.Description;

            if (updateDto.ShortDescription != null)
                existingEntity.ShortDescription = updateDto.ShortDescription;

            if (updateDto.Price.HasValue)
                existingEntity.Price = updateDto.Price.Value;

            if (updateDto.SalePrice.HasValue)
                existingEntity.SalePrice = updateDto.SalePrice.Value;

            if (updateDto.GenderTarget.HasValue)
                existingEntity.GenderTarget = updateDto.GenderTarget.Value;

            if (updateDto.CategoryId.HasValue)
                existingEntity.CategoryId = updateDto.CategoryId.Value;

            if (updateDto.BrandId.HasValue)
                existingEntity.BrandId = updateDto.BrandId.Value;

            if (updateDto.MetaTitle != null)
                existingEntity.MetaTitle = updateDto.MetaTitle;

            if (updateDto.MetaDescription != null)
                existingEntity.MetaDescription = updateDto.MetaDescription;

            await BeforeUpdateAsync(existingEntity);

            var updatedEntityEntry = await _productRepository.UpdateAsync(existingEntity);
            var updatedEntity = updatedEntityEntry.Entity;
            await _productRepository.SaveChangesAsync();

            await AfterUpdateAsync(updatedEntity);

            return OperationResult<ProductDto>.Success(MapToProductDto(updatedEntity));
        }

        public override async Task<OperationResult<ProductDto>> CreateAsync(ProductCreateDto createDto)
        {
            try
            {
                await ValidateCreateAsync(createDto);

                var entity = MapToProduct(createDto);

                await BeforeCreateAsync(entity);

                var createdEntityEntry = await _productRepository.AddAsync(entity);
                var createdEntity = createdEntityEntry.Entity;
                await _productRepository.SaveChangesAsync();
                await AfterCreateAsync(createdEntity);

                return OperationResult<ProductDto>.Success(MapToProductDto(createdEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return OperationResult<ProductDto>.Fail("Error creating product: " + ex.Message);
            }
        }


        public override async Task<OperationResult<ProductDto>> GetByIdAsync(Guid id, params Expression<Func<Product, object>>[] includes)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id, includes);
                if (product == null)
                    return OperationResult<ProductDto>.Fail("Product not found");

                var dto = MapToProductDto(product);
                return OperationResult<ProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id {Id} with includes", id);
                return OperationResult<ProductDto>.Fail("Error getting product by id: " + ex.Message);
            }
        }

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
                    .Select(p => new ProductDto
                    {
                        Name = p.Name,
                        Description = p.Description,
                        ShortDescription = p.ShortDescription,
                        Sku = p.Sku,
                        Price = p.Price,
                        SalePrice = p.SalePrice,
                        GenderTarget = p.GenderTarget,
                        MetaTitle = p.MetaTitle,
                        MetaDescription = p.MetaDescription,
                        CategoryId = p.CategoryId,
                        BrandId = p.BrandId,
                        CategoryName = p.Category != null ? p.Category.Name : "No Category",
                        BrandName = p.Brand != null ? p.Brand.Name : "No Brand",

                        Category = new CategoryDto
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name
                        },

                        Images = p.Images.Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            AltText = i.AltText
                        }).ToList(),
                        Variants = p.Variants.Select(v => new ProductVariantDto
                        {
                            Id = v.Id,
                            Size = v.Size,
                            Color = v.Color,
                            StockQuantity = v.StockQuantity
                        }).ToList(),
                        Reviews = p.Reviews.Select(r => new ReviewDto
                        {
                            Id = r.Id,
                            Rating = r.Rating,
                            ReviewText = r.ReviewText
                        }).ToList()
                    })
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
                products = products.Where(p => p.SalePrice > 0).ToList();

                var salesProducts = products
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new ProductDto
                    {
                        Name = p.Name,
                        Description = p.Description,
                        ShortDescription = p.ShortDescription,
                        Sku = p.Sku,
                        Price = p.Price,
                        SalePrice = p.SalePrice,
                        GenderTarget = p.GenderTarget,
                        MetaTitle = p.MetaTitle,
                        MetaDescription = p.MetaDescription,
                        CategoryId = p.CategoryId,
                        BrandId = p.BrandId,
                        CategoryName = p.Category != null ? p.Category.Name : "No Category",
                        BrandName = p.Brand != null ? p.Brand.Name : "No Brand",

                        Category = new CategoryDto
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name
                        },

                        Images = p.Images.Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            AltText = i.AltText
                        }).ToList(),
                        Variants = p.Variants.Select(v => new ProductVariantDto
                        {
                            Id = v.Id,
                            Size = v.Size,
                            Color = v.Color,
                            StockQuantity = v.StockQuantity
                        }).ToList(),
                        Reviews = p.Reviews.Select(r => new ReviewDto
                        {
                            Id = r.Id,
                            Rating = r.Rating,
                            ReviewText = r.ReviewText
                        }).ToList()
                    })
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



    }
}
