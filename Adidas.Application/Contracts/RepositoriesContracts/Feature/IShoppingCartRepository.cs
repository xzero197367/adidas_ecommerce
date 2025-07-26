using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Feature
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        Task<IEnumerable<ShoppingCart>> GetCartItemsByUserIdAsync(string userId);
        Task<ShoppingCart?> GetCartItemAsync(string userId, Guid variantId);
        Task<bool> ClearCartAsync(string userId);
        Task<decimal> CalculateTotalCostAsync(string userId);
        Task<int> CountCartItemsAsync(string userId);
    }
}
