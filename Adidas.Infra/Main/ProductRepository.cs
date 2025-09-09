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
                .Include(p => p.Variants)
                .AsQueryable();

            // Category filter
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            // Brand filter
            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId.Value);

            // Gender filter
            if (filter.Gender.HasValue)
                query = query.Where(p => p.GenderTarget == filter.Gender.Value);

            // Price range
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // On Sale
            if (filter.IsOnSale.HasValue)
            {
                query = filter.IsOnSale.Value
                    ? query.Where(p => p.SalePrice!=null)
                    : query.Where(p => p.SalePrice == null);
            }

            // In Stock
            if (filter.InStock.HasValue)
            {
                query = filter.InStock.Value
                    ? query.Where(p => p.Variants.Any(v => v.StockQuantity > 0))
                    : query.Where(p => p.Variants.All(v => v.StockQuantity == 0));
            }
            // Min stock
            if (filter.MinStock.HasValue)
                query = query.Where(p => p.Variants.Sum(v => v.StockQuantity) >= filter.MinStock.Value);

            // Max stock
            if (filter.MaxStock.HasValue)
                query = query.Where(p => p.Variants.Sum(v => v.StockQuantity) <= filter.MaxStock.Value);

            // Featured & Active
            // if (filter.IsFeatured.HasValue)
            //     query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            // Date range
            if (filter.CreatedAfter.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter.Value);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore.Value);

            // Search term (case-insensitive, across multiple fields)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term) ||
                    p.Brand.Name.ToLower().Contains(term) ||
                    p.Category.Name.ToLower().Contains(term));
            }

            // Total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "price" => filter.SortDescending
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price),

                    "name" => filter.SortDescending
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name),

                    "createdat" => filter.SortDescending
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),

                    _ => query.OrderBy(p => p.Name) // default sort
                };
            }
            else
            {
                query = query.OrderBy(p => p.Name); // fallback default
            }

            // Paging
            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<Product?> GetProductWithVariantsAsync(Guid productId)
        {
            // i want to return the reviews just approved
            return await _context.Products
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Images)
                .Include(p => p.Reviews.Where(r => r.IsApproved)) // Include only approved reviews
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
            //return await _context.Products
            //    .Include(p => p.Variants)
            //        .ThenInclude(v => v.Images)
            //    .Include(p => p.Images)

            //    .Include(p => p.Reviews )
            //    .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
        }

        public async Task<List<Product>> GetByIdsAsync(List<Guid> productIds) =>
       await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();


    }
}
