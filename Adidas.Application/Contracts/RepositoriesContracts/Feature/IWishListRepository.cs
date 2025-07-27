using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Feature
{
    public interface IWishListRepository : IGenericRepository<Wishlist>
    {
        Task<IEnumerable<Wishlist>> GetWishListByUserIdAsync(string userId);
        Task<bool> IsProductInWishListAsync(string userId, Guid productId);
        Task<bool> RemoveFromWishListAsync(string userId , Guid productId);
        Task<int> GetWishListCountAsync(string userId);

    }
}
