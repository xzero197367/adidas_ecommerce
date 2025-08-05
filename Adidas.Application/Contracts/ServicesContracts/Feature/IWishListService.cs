using Adidas.DTOs.Feature.WishLIstDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IWishListService
    {
        Task<IEnumerable<WishlistItemDto>> GetWishlistByUserIdAsync(string userId);
        Task<WishlistItemDto> AddToWishlistAsync(WishlistCreateDto addDto);
        Task<bool> RemoveFromWishlistAsync(string userId, Guid productId);
        Task<bool> IsProductInWishlistAsync(string userId, Guid productId);
        Task<int> GetWishlistCountAsync(string userId);
        Task<IEnumerable<WishlistItemDto>> GetWishlistSummaryAsync(string userId);
        Task<bool> MoveToCartAsync(string userId, Guid productId, Guid variantId);
        Task<string> GenerateWishlistShareLinkAsync(string userId);
        Task<IEnumerable<WishlistItemDto>> GetSharedWishlistAsync(string shareToken);
        Task<bool> NotifyWhenInStockAsync(string userId, Guid productId);
        Task<bool> RemoveStockNotificationAsync(string userId, Guid productId);
    }
}
