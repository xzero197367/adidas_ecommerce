using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(Guid brandId);
        Task<IEnumerable<Product>> GetProductsByGenderAsync(Gender gender);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();
        Task<IEnumerable<Product>> GetProductsOnSaleAsync();
        Task<Product?> GetProductBySkuAsync(string sku);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<(IEnumerable<Product> products, int totalCount)> GetProductsWithFiltersAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, Guid? brandId = null,
            Gender? gender = null, decimal? minPrice = null, decimal? maxPrice = null,
            string? searchTerm = null);
    }
}
