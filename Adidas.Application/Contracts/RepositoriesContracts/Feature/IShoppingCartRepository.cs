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
        Task<IEnumerable<ShoppingCart>> GetCartItemsByUserIdAsync(Guid userId);
        Task<ShoppingCart?> GetCartItemAsync(Guid userId, Guid variantId);
        Task<bool> ClearCartAsync(Guid userId);
        Task<decimal> CalculateTotalCostAsync(Guid userId);
        Task<int> CountCartItemsAsync(Guid userId);
    }
}
