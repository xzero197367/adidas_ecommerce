using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class ShoppingCartService : GenericService<ShoppingCart, ShoppingCartDto, ShoppingCartCreateDto, ShoppingCartUpdateDto>,IShoppingCartService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ILogger<OrderCouponService> logger;


        public ShoppingCartService(
            IShoppingCartRepository cartRepository,
            ILogger<OrderCouponService> logger,
            IWishlistRepository wishlistRepository): base(cartRepository, logger)
        {
            this.logger = logger;
            _cartRepository = cartRepository;
            _wishlistRepository = wishlistRepository;
        }

        public async Task<OperationResult<ShoppingCartDto>> AddToCartAsync(ShoppingCartCreateDto addCreateDto)
        {
            try
            {
                var existingItem =
                    await _cartRepository.GetCartItemAsync(addCreateDto.UserId, addCreateDto.ProductVariantId);

                if (existingItem != null)
                {
                    existingItem.Quantity += addCreateDto.Quantity;
                    await _cartRepository.UpdateAsync(existingItem);
                }
                else
                {
                    var cartItem = new ShoppingCart
                    {
                        UserId = addCreateDto.UserId,
                        VariantId = addCreateDto.ProductVariantId,
                        Quantity = addCreateDto.Quantity
                    };
                    await _cartRepository.AddAsync(cartItem);

                }

                var updatedItem =
                    await _cartRepository.GetCartItemAsync(addCreateDto.UserId, addCreateDto.ProductVariantId);
                var result = await _cartRepository.SaveChangesAsync();
                if (result == 0) return OperationResult<ShoppingCartDto>.Fail("Failed to save cart item");
                return OperationResult<ShoppingCartDto>.Success(updatedItem.Adapt<ShoppingCartDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding item to cart");
                return OperationResult<ShoppingCartDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> ClearCartAsync(string userId)
        {
            try
            {
                var result = await _cartRepository.ClearCartAsync(userId);
                return OperationResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing cart");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> MergeCartsAsync(string fromUserId, string toUserId)
        {
            try
            {
                var fromCart = await _cartRepository.GetCartItemsByUserIdAsync(fromUserId);
                var toCart = await _cartRepository.GetCartItemsByUserIdAsync(toUserId);

                foreach (var item in fromCart)
                {
                    var existing = toCart.FirstOrDefault(c => c.VariantId == item.VariantId);
                    if (existing != null)
                    {
                        existing.Quantity += item.Quantity;
                        await _cartRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        item.UserId = toUserId;
                        await _cartRepository.AddAsync(item);
                    }
                }

                var result = await _cartRepository.ClearCartAsync(fromUserId);
                return OperationResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error merging carts");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetCartItemsByUserIdAsync(string userId)
        {
            try
            {
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                return OperationResult<IEnumerable<ShoppingCartDto>>.Success(
                    items.Adapt<IEnumerable<ShoppingCartDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting cart items");
                return OperationResult<IEnumerable<ShoppingCartDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryAsync(string userId)
        {
            try
            {
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                var dtoItems = items.Adapt<IEnumerable<ShoppingCartDto>>();
                var summary = dtoItems.Adapt<ShoppingCartSummaryDto>();
                return OperationResult<ShoppingCartSummaryDto>.Success(summary);
                ;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting cart summary");
                return null;
            }
        }

        public async Task<OperationResult<bool>> MoveToWishlistAsync(string userId, Guid variantId)
        {
            try
            {
                var item = await _cartRepository.GetCartItemAsync(userId, variantId);
                if (item == null) return OperationResult<bool>.Fail("Item not found");

                await _wishlistRepository.AddAsync(new Wishlist
                {
                    UserId = userId,
                    ProductId = item.Variant.ProductId,
                    AddedAt = DateTime.UtcNow
                });

                var result = await _cartRepository.RemoveFromCartAsync(userId, variantId);
                return OperationResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error moving item to wishlist");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> RemoveFromCartAsync(string userId, Guid variantId)
        {
            try
            {
                var result = await _cartRepository.RemoveFromCartAsync(userId, variantId);
                return OperationResult<bool>.Success(result);
            }
            catch
            {
                logger.LogError("Error removing item from cart");
                return OperationResult<bool>.Fail("Error removing item from cart");
            }
        }

        public async Task<OperationResult<bool>> RestoreSavedCartAsync(string userId)
        {
            try
            {
                // Custom logic needed if SaveCartForLaterAsync is implemented with a separate table
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error restoring saved cart");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> SaveCartForLaterAsync(string userId)
        {
            try
            {
                // Custom logic needed if SaveCartForLaterAsync is implemented with a separate table
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving cart for later");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> ValidateCartItemsAsync(string userId)
        {
            try
            {
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating cart items");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetUnavailableItemsAsync(string userId)
        {
            try
            {
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                var unavailable = items.Where(i => i.Variant.StockQuantity < i.Quantity);
                
                return OperationResult<IEnumerable<ShoppingCartDto>>.Success(unavailable
                    .Adapt<IEnumerable<ShoppingCartDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting unavailable items");
                return OperationResult<IEnumerable<ShoppingCartDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartDto>> UpdateCartItemQuantityAsync(
            ShoppingCartUpdateDto shoppingCartUpdateDto)
        {
            try
            {
                var item = await _cartRepository.GetByIdAsync(shoppingCartUpdateDto.Id);
                if (item == null) return OperationResult<ShoppingCartDto>.Fail("Item not found");
                var result = await _cartRepository.UpdateAsync(shoppingCartUpdateDto.Adapt<ShoppingCart>());
                await _cartRepository.SaveChangesAsync();
                result.State = EntityState.Detached;
                return OperationResult<ShoppingCartDto>.Success(result.Entity.Adapt<ShoppingCartDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating cart item quantity");
                return OperationResult<ShoppingCartDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryWithTaxAsync(string userId,
            string? shippingAddress = null)
        {
            try
            {
                var summary = await GetCartSummaryAsync(userId);
                if (summary.IsSuccess)
                {
                    summary.Data.TaxAmount = summary.Data.Subtotal * 0.15m;
                    summary.Data.ShippingCost = 30m;
                    summary.Data.TotalAmount = summary.Data.Subtotal + summary.Data.TaxAmount + summary.Data.ShippingCost;
                }
                return summary;
            }catch (Exception ex)
            {
                logger.LogError(ex, "Error getting cart summary with tax");
                return OperationResult<ShoppingCartSummaryDto>.Fail(ex.Message);
            }
        }
    }
}