
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.Main
{
    public class ProductService : GenericService<Product, ProductDto, CreateProductDto, UpdateProductDto>, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICouponService _couponService;

        public ProductService(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            ICouponService couponService,
            IMapper mapper,
            ILogger<ProductService> logger)
            : base(productRepository, mapper, logger)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _couponService = couponService;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(Guid brandId)
        {
            var products = await _productRepository.GetProductsByBrandAsync(brandId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        public async Task<ProductDto?> GetProductWithVariantsAsync(Guid productId)
        {
            var product = await _productRepository.GetProductWithVariantsAsync(productId);
            if (product == null) return null;
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByGenderAsync(Gender gender)
        {
            var products = await _productRepository.GetProductsByGenderAsync(gender);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
        {
            var products = await _productRepository.GetFeaturedProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        public async Task<ProductVariantDto?> GetVariantByIdAsync(Guid id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return null;
            return _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task DeleteVariantAsync(Guid id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return;

            _variantRepository.Remove(variant);
            await _variantRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsOnSaleAsync()
        {
            var products = await _productRepository.GetProductsOnSaleAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductBySkuAsync(string sku)
        {
            var product = await _productRepository.GetProductBySkuAsync(sku);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsWithFiltersAsync(ProductFilterDto filters)
        {
            var (products, totalCount) = await _productRepository.GetProductsWithFiltersAsync(
                filters.PageNumber, filters.PageSize, filters.CategoryId, filters.BrandId,
                filters.Gender, filters.MinPrice, filters.MaxPrice, filters.SearchTerm);

            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(products),
                TotalCount = totalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };
        }

        public async Task<bool> UpdateInventoryAsync(Guid productId, Dictionary<Guid, int> variantStockUpdates)
        {
            try
            {
                foreach (var update in variantStockUpdates)
                {
                    await _variantRepository.UpdateStockAsync(update.Key, update.Value);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory for product {ProductId}", productId);
                return false;
            }
        }

        public async Task<decimal> CalculateDiscountedPriceAsync(Guid productId, string? discountCode = null)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return 0;

            var basePrice = product.SalePrice ?? product.Price;

            if (string.IsNullOrEmpty(discountCode))
                return basePrice;

            var discountAmount = await _couponService.CalculateCouponAmountAsync(discountCode, basePrice);
            return Math.Max(0, basePrice - discountAmount);
        }
        public async Task<PagedResultDto<ProductDto>> GetProductsFilteredByCategoryBrandGenderAsync(ProductFilterDto filters)
        {
            var (products, totalCount) = await _productRepository.GetFilteredProductsAsync(filters);

            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(products),
                TotalCount = totalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };
        }


        protected override Task ValidateCreateAsync(CreateProductDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("Product name is required");

            if (createDto.Price <= 0)
                throw new ArgumentException("Product price must be greater than 0");

            return Task.CompletedTask;
        }

        protected override async Task BeforeCreateAsync(Product entity)
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
