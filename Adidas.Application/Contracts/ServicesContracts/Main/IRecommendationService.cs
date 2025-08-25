using Adidas.DTOs.Main.Product_DTOs;
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
        Task<List<ProductDto?>> GetRecommendationsAsync(Guid productId);

    }
}
