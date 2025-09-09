using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Main;
using Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Main
{
    // Application/Services/RecommendationService.cs
    public class RecommendationService : IRecommendationService
    {
        private readonly IUserProductViewRepository _viewRepo;
        private readonly IProductRepository _productRepo;

        public RecommendationService(IUserProductViewRepository viewRepo, IProductRepository productRepo)
        {
            _viewRepo = viewRepo;
            _productRepo = productRepo;
        }

        public async Task<List<ProductDto?>> GetRecommendationsAsync(Guid productId)
        {
            var views = await _viewRepo.GetByProductIdAsync(productId);
            var userIds = views.Select(v => v.UserId).Distinct().ToList();

            if (!userIds.Any())
                return new List<ProductDto?>();

            var allUserViews = await _viewRepo.GetByUserIdsAsync(userIds);


            // Step 3: Group other products they viewed
            var relatedProductIds = allUserViews
                .Where(v => v.ProductId != productId)
                .GroupBy(v => v.ProductId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToList();

            if (!relatedProductIds.Any())
                return new List<ProductDto?>();

            var recommendedProducts = await _productRepo.GetByIdsAsync(relatedProductIds);

            return recommendedProducts.Select(MapToProductDto).ToList();
        }

        private ProductDto MapToProductDto(Product p)
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
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                IsActive = p.IsActive,
                CategoryName = p.Category?.Name,
                BrandName = p.Brand?.Name,

                Category = p.Category != null ? new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name
                } : null,

                Images = p.Images?.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText
                }).ToList() ?? new List<ProductImageDto>(),

                Variants = p.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Color = v.Color,
                    Size = v.Size,
                    StockQuantity = v.StockQuantity,
                    PriceAdjustment = v.PriceAdjustment
                }).ToList() ?? new List<ProductVariantDto>(),

                Reviews = p.Reviews?.Select(r => new Review
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    IsApproved = r.IsApproved,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    Title = r.Title,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText
                }).ToList() ?? new List<Review>(),

                InStock = p.Variants?.Any(v => v.StockQuantity > 0) ?? false
            };
        }
    }
} 