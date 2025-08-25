using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Models.Main;
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

        public async Task<List<Product>> GetRecommendationsAsync(Guid productId)
        {
            var views = await _viewRepo.GetByProductIdAsync(productId);
            var userIds = views.Select(v => v.UserId).Distinct().ToList();

            var otherViews = views
                .Where(v => v.ProductId != productId && userIds.Contains(v.UserId))
                .GroupBy(v => v.ProductId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToList();

            var recommendedProducts = await _productRepo.GetByIdsAsync(otherViews);

            return recommendedProducts;
        }
    }

}
