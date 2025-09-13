using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductService : IGenericService<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
    {
        public ProductDto MapToProductDto(Product p);
        Task<ProductVariantDto?> GetVariantByIdAsync(Guid id);
        Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(Guid categoryId);
        Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByBrandAsync(Guid brandId);
        Task<OperationResult<IEnumerable<ProductDto>>> GetProductsByGenderAsync(Gender gender);
        Task<OperationResult<IEnumerable<ProductDto>>> GetFeaturedProductsAsync();
        Task<OperationResult<IEnumerable<ProductDto>>> GetProductsOnSaleAsync();
        Task<OperationResult<ProductDto>> GetProductBySkuAsync(string sku);
        Task<OperationResult<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm);
        Task<OperationResult<PagedResultDto<ProductDto>>> GetProductsWithFiltersAsync(ProductFilterDto filters);
        Task<OperationResult<bool>> UpdateInventoryAsync(Guid productId, Dictionary<Guid, int> variantStockUpdates);
        Task<OperationResult<decimal>> CalculateDiscountedPriceAsync(Guid productId, string? discountCode = null);
        Task<OperationResult<IEnumerable<ProductDto>>> GetPreviouslyPurchasedProductsForAllUsersAsync();
        Task<PagedResultDto<ProductDto>> GetProductsFilteredByCategoryBrandGenderAsync(ProductFilterDto filters);
        Task<ProductDto?> GetProductWithVariantsAsync(Guid productId);
        Task DeleteVariantAsync(Guid id);
        Task<OperationResult<List<ProductDto>>> GetLastAddedProducts();
        Task<OperationResult<List<ProductDto>>> GetSalesProducts();
        Task<ProductDto?> GetProductWithVariantsAsync(Guid id, string? userId);
    }
}