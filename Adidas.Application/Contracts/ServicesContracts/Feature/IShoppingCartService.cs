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
        Task<IEnumerable<ShoppingCartDto>> GetCartItemsByUserIdAsync(string userId);
        Task<ShoppingCartDto> AddToCartAsync(ShoppingCartCreateDto addCreateDto);
        Task<ShoppingCartDto> UpdateCartItemQuantityAsync(ShoppingCartUpdateDto shoppingCartUpdateDto);
        Task<bool> RemoveFromCartAsync(string userId, Guid variantId);
        Task<bool> ClearCartAsync(string userId);

        Task<ShoppingCartSummaryDto> GetCartSummaryAsync(string userId);
        Task<ShoppingCartSummaryDto> GetCartSummaryWithTaxAsync(string userId, string? shippingAddress = null);
        Task<bool> ValidateCartItemsAsync(string userId);
        Task<IEnumerable<ShoppingCartDto>> GetUnavailableItemsAsync(string userId);

        Task<bool> MergeCartsAsync(string fromUserId, string toUserId);
        Task<bool> SaveCartForLaterAsync(string userId);
        Task<bool> RestoreSavedCartAsync(string userId);

        //Task<bool> MoveToWishlistAsync(Guid userId, Guid variantId);
        //Task<bool> MoveFromWishlistAsync(Guid userId, Guid productId, Guid variantId);
    }
}

