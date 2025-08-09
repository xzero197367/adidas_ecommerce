using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.DTOs.Main.ProductDTOs;
using Adidas.Models.Main;
using Models.People;
using Microsoft.EntityFrameworkCore; 

namespace Adidas.Infra.Main
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive && p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandAsync(Guid brandId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive && p.BrandId == brandId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByGenderAsync(Gender gender)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive && p.GenderTarget == gender)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
        {
            // Assuming you have a way to mark featured products or use top-rated products
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsOnSaleAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive && p.SalePrice.HasValue && p.SalePrice < p.Price)
                .ToListAsync();
        }

        public async Task<Product?> GetProductBySkuAsync(string sku)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Sku == sku);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.ShortDescription.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product> products, int totalCount)> GetProductsWithFiltersAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, Guid? brandId = null,
            Gender? gender = null, decimal? minPrice = null, decimal? maxPrice = null,
            string? searchTerm = null)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            if (gender.HasValue)
                query = query.Where(p => p.GenderTarget == gender);

            if (minPrice.HasValue)
                query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                        p.Description.Contains(searchTerm) ||
                                        p.ShortDescription.Contains(searchTerm));

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((pageNumber - 1) *pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            var query = _dbSet
     .Include(p => p.Category)
     .Include(p => p.Brand)
     .Include(p => p.Images)
       .Include(p => p.Variants)
     .AsQueryable(); 


            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId);

            if (filter.Gender.HasValue)
                query = query.Where(p => p.GenderTarget == filter.Gender.Value);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (products, totalCount);
        }
        public async Task<Product?> GetProductWithVariantsAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
        }


    }
}
