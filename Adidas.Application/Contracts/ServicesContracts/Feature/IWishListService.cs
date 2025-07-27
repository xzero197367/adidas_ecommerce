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
        Task<IEnumerable<WishlistItemDto>> GetWishlistByUserIdAsync(Guid userId);
        Task<WishlistItemDto> AddToWishlistAsync(AddToWishlistDto addDto);
        Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId);
        Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId);
        Task<int> GetWishlistCountAsync(Guid userId);
        Task<IEnumerable<WishlistItemDto>> GetWishlistSummaryAsync(Guid userId);
        Task<bool> MoveToCartAsync(Guid userId, Guid productId, Guid variantId);
        Task<string> GenerateWishlistShareLinkAsync(Guid userId);
        Task<IEnumerable<WishlistItemDto>> GetSharedWishlistAsync(string shareToken);
        Task<bool> NotifyWhenInStockAsync(Guid userId, Guid productId);
        Task<bool> RemoveStockNotificationAsync(Guid userId, Guid productId);
    }
}
