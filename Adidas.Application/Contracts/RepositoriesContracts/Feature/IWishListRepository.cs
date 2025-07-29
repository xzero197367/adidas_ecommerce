

using Models.Feature;

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
