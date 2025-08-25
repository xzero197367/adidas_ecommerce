using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IRecommendationService
    {
        Task<List<Product>> GetRecommendationsAsync(Guid productId);

    }
}
