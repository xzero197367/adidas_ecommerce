using Adidas.DTOs.Feature.WishLIstDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IWishListService
    {
        Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistByUserIdAsync(string userId);
        Task<OperationResult<WishlistDto>> AddToWishlistAsync(WishlistCreateDto addDto);
        Task<OperationResult<bool>> RemoveFromWishlistAsync(string userId, Guid productId);
        Task<OperationResult<bool>> IsProductInWishlistAsync(string userId, Guid productId);
        Task<OperationResult<int>> GetWishlistCountAsync(string userId);
        Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistSummaryAsync(string userId);
        // Task<OperationResult<bool>> MoveToCartAsync(string userId, Guid productId, Guid variantId);
        // Task<OperationResult<string>> GenerateWishlistShareLinkAsync(string userId);
        // Task<OperationResult<IEnumerable<WishlistDto>>> GetSharedWishlistAsync(string shareToken);
        // Task<OperationResult<bool>> NotifyWhenInStockAsync(string userId, Guid productId);
        // Task<OperationResult<bool>> RemoveStockNotificationAsync(string userId, Guid productId);
    }
}
