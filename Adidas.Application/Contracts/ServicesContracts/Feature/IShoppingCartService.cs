using Adidas.DTOs.Feature.ShoppingCartDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IShoppingCartService 
    {
        Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsByUserIdAsync(string userId);
        Task<ShoppingCartItemDto> AddToCartAsync(ShoppingCartCreateDto addCreateDto);
        Task<ShoppingCartItemDto> UpdateCartItemQuantityAsync(ShoppingChartUpdateDto shoppingChartUpdateDto);
        Task<bool> RemoveFromCartAsync(string userId, Guid variantId);
        Task<bool> ClearCartAsync(string userId);

        Task<CartSummaryDto> GetCartSummaryAsync(string userId);
        Task<CartSummaryDto> GetCartSummaryWithTaxAsync(string userId, string? shippingAddress = null);
        Task<bool> ValidateCartItemsAsync(string userId);
        Task<IEnumerable<ShoppingCartItemDto>> GetUnavailableItemsAsync(string userId);

        Task<bool> MergeCartsAsync(string fromUserId, string toUserId);
        Task<bool> SaveCartForLaterAsync(string userId);
        Task<bool> RestoreSavedCartAsync(string userId);

        //Task<bool> MoveToWishlistAsync(Guid userId, Guid variantId);
        //Task<bool> MoveFromWishlistAsync(Guid userId, Guid productId, Guid variantId);
    }
}

