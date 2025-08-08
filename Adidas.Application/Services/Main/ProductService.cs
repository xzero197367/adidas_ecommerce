using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.Extensions.Logging;
using Models.People;

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

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(Guid categoryId)
        {
            try
            {
                var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
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
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
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
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
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
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
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
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
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
                    : OperationResult<ProductDto>.Success(product.Adapt<ProductDto>());
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
                return OperationResult<IEnumerable<ProductDto>>.Success(products.Adapt<IEnumerable<ProductDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return OperationResult<IEnumerable<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<PagedResultDto<ProductDto>>> GetProductsWithFiltersAsync(
            ProductFilterDto filters)
        {
            try
            {
                var (products, totalCount) = await _productRepository.GetProductsWithFiltersAsync(
                    filters.PageNumber, filters.PageSize, filters.CategoryId, filters.BrandId,
                    filters.Gender, filters.MinPrice, filters.MaxPrice, filters.SearchTerm);

                var PagedResultDto = new PagedResultDto<ProductDto>
                {
                    Items = products.Adapt<IEnumerable<ProductDto>>(),
                    TotalCount = totalCount,
                    PageNumber = filters.PageNumber,
                    PageSize = filters.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
                };

                return OperationResult<PagedResultDto<ProductDto>>.Success(PagedResultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products with filters");
                return OperationResult<PagedResultDto<ProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> UpdateInventoryAsync(Guid productId,
            Dictionary<Guid, int> variantStockUpdates)
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

        public async Task<OperationResult<decimal>> CalculateDiscountedPriceAsync(Guid productId,
            string? discountCode = null)
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
            // Generate SKU if not provided
            if (string.IsNullOrEmpty(entity.Sku))
            {
                entity.Sku = await GenerateSkuAsync(entity.Name);
            }

            // Create slug from name
            //entity.Slug = CreateSlug(entity.Name);
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

        private static string CreateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("&", "and");
        }
    }
}