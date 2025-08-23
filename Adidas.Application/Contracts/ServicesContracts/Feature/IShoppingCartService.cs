using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IShoppingCartService : IGenericService<ShoppingCart, ShoppingCartDto, ShoppingCartCreateDto, ShoppingCartUpdateDto>
    {
        Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetCartItemsByUserIdAsync(string userId);
        Task<OperationResult<ShoppingCartDto>> AddToCartAsync(ShoppingCartCreateDto addCreateDto);
        Task<OperationResult<ShoppingCartDto>> UpdateCartItemQuantityAsync(ShoppingCartUpdateDto shoppingCartUpdateDto);
        Task<OperationResult<bool>> RemoveFromCartAsync(string userId, Guid variantId);
        Task<OperationResult<bool>> ClearCartAsync(string userId);

        Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryAsync(string userId);
        Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryWithTaxAsync(string userId, string? shippingAddress = null);
        Task<OperationResult<bool>> ValidateCartItemsAsync(string userId);
        Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetUnavailableItemsAsync(string userId);

        Task<OperationResult<bool>> MergeCartsAsync(string fromUserId, string toUserId);
        Task<OperationResult<bool>> SaveCartForLaterAsync(string userId);
        Task<OperationResult<bool>> RestoreSavedCartAsync(string userId);
        Task<OperationResult<bool>> MoveToWishlistAsync(string userId, Guid variantId);
    }
}

