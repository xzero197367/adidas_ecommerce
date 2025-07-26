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
        Task<IEnumerable<Wishlist>> GetWishListByUserIdAsync(Guid userId);
        Task<bool> IsProductInWishListAsync(Guid userId, Guid productId);
        Task<bool> RemoveFromWishListAsync(Guid userId , Guid productId);
        Task<int> GetWishListCountAsync(Guid userId);

    }
}
