using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductService : IGenericService<Product, ProductDto, CreateProductDto, UpdateProductDto>
    {
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(Guid brandId);
        Task<IEnumerable<ProductDto>> GetProductsByGenderAsync(Gender gender);
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsOnSaleAsync();
        Task<ProductDto?> GetProductBySkuAsync(string sku);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<PagedResultDto<ProductDto>> GetProductsWithFiltersAsync(ProductFilterDto filters);
        Task<bool> UpdateInventoryAsync(Guid productId, Dictionary<Guid, int> variantStockUpdates);
        Task<decimal> CalculateDiscountedPriceAsync(Guid productId, string? discountCode = null);
    }
}
