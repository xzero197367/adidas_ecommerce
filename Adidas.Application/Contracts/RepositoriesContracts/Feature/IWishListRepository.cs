

using Models.Feature;

namespace Adidas.Application.Contracts.RepositoriesContracts.Feature
{
    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(string userId);
        Task<bool> IsProductInWishlistAsync(string userId, Guid productId);
        Task<bool> RemoveFromWishlistAsync(string userId, Guid productId);
        Task<int> GetWishlistCountAsync(string userId);
    }
}
