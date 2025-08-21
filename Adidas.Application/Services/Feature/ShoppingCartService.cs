using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Feature
{
    public class ShoppingCartService : GenericService<ShoppingCart, ShoppingCartDto, ShoppingCartCreateDto, ShoppingCartUpdateDto>, IShoppingCartService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ILogger<ShoppingCartService> _logger; // Fixed logger type

        public ShoppingCartService(
            IShoppingCartRepository cartRepository,
            ILogger<ShoppingCartService> logger, // Updated logger type
            IWishlistRepository wishlistRepository) : base(cartRepository, logger)
        {
            _logger = logger;
            _cartRepository = cartRepository;
            _wishlistRepository = wishlistRepository;
        }

        public async Task<OperationResult<ShoppingCartDto>> AddToCartAsync(ShoppingCartCreateDto addCreateDto)
        {
            try
            {
                var existingItem = await _cartRepository.GetCartItemAsync(addCreateDto.UserId, addCreateDto.ProductVariantId);

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

                var updatedItem = await _cartRepository.GetCartItemAsync(addCreateDto.UserId, addCreateDto.ProductVariantId);
                return OperationResult<ShoppingCartDto>.Success(MapToShoppingCartDto(updatedItem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", addCreateDto.UserId);
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
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
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
                _logger.LogError(ex, "Error merging carts from {FromUserId} to {ToUserId}", fromUserId, toUserId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetCartItemsByUserIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving cart items for user {UserId}", userId);
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                var dtos = items.Select(MapToShoppingCartDto).ToList();
                _logger.LogInformation("Successfully retrieved {Count} cart items for user {UserId}", dtos.Count, userId);
                return OperationResult<IEnumerable<ShoppingCartDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items for user {UserId}", userId);
                return OperationResult<IEnumerable<ShoppingCartDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving cart summary for user {UserId}", userId);
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                var dtos = items.Select(MapToShoppingCartDto).ToList();
                var summary = MapToShoppingCartSummaryDto(dtos, userId);
                _logger.LogInformation("Successfully retrieved cart summary for user {UserId}", userId);
                return OperationResult<ShoppingCartSummaryDto>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for user {UserId}", userId);
                return OperationResult<ShoppingCartSummaryDto>.Fail(ex.Message);
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
                _logger.LogError(ex, "Error moving item to wishlist for user {UserId}, variant {VariantId}", userId, variantId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user {UserId}, variant {VariantId}", userId, variantId);
                return OperationResult<bool>.Fail(ex.Message);
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
                _logger.LogError(ex, "Error restoring saved cart for user {UserId}", userId);
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
                _logger.LogError(ex, "Error saving cart for later for user {UserId}", userId);
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
                _logger.LogError(ex, "Error validating cart items for user {UserId}", userId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ShoppingCartDto>>> GetUnavailableItemsAsync(string userId)
        {
            try
            {
                var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
                var unavailable = items.Where(i => i.Variant.StockQuantity < i.Quantity);
                var dtos = unavailable.Select(MapToShoppingCartDto).ToList();
                return OperationResult<IEnumerable<ShoppingCartDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unavailable items for user {UserId}", userId);
                return OperationResult<IEnumerable<ShoppingCartDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartDto>> UpdateCartItemQuantityAsync(ShoppingCartUpdateDto shoppingCartUpdateDto)
        {
            try
            {
                var item = await _cartRepository.GetByIdAsync(shoppingCartUpdateDto.Id);
                if (item == null) return OperationResult<ShoppingCartDto>.Fail("Item not found");

                if (shoppingCartUpdateDto.Quantity.HasValue)
                    item.Quantity = shoppingCartUpdateDto.Quantity.Value;
                if (shoppingCartUpdateDto.VariantId.HasValue)
                    item.VariantId = shoppingCartUpdateDto.VariantId.Value;

                var result = await _cartRepository.UpdateAsync(item);
                await _cartRepository.SaveChangesAsync();
                return OperationResult<ShoppingCartDto>.Success(MapToShoppingCartDto(result.Entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item quantity for id {Id}", shoppingCartUpdateDto.Id);
                return OperationResult<ShoppingCartDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<ShoppingCartSummaryDto>> GetCartSummaryWithTaxAsync(string userId, string? shippingAddress = null)
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary with tax for user {UserId}", userId);
                return OperationResult<ShoppingCartSummaryDto>.Fail(ex.Message);
            }
        }

        private ShoppingCartDto MapToShoppingCartDto(ShoppingCart cart)
        {
            if (cart == null) return null;

            return new ShoppingCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                VariantId = cart.VariantId,
                Quantity = cart.Quantity,
                UnitPrice = cart.Variant?.Product?.Price ?? 0m,
                SalePrice = cart.Variant?.Product?.SalePrice ?? 0m,
                Variant = cart.Variant != null ? new ProductVariantDto
                {
                    Id = cart.Variant.Id,
                    ProductId = cart.Variant.ProductId,
                    StockQuantity = cart.Variant.StockQuantity,
                    Sku = cart.Variant.Sku,
                    Size = cart.Variant.Size,
                    Color = cart.Variant.Color,
                    PriceAdjustment = cart.Variant.PriceAdjustment,
                    // Explicitly exclude Product and ShoppingCarts to avoid circular references
                } : null,
                CreatedAt = cart.AddedAt,
                UpdatedAt = cart.UpdatedAt,
                IsActive = cart.IsActive,
            };
        }

        private ShoppingCartSummaryDto MapToShoppingCartSummaryDto(IEnumerable<ShoppingCartDto> dtos, string userId)
        {
            return new ShoppingCartSummaryDto
            {
                UserId = userId,
                ItemCount = dtos.Count(),
                TotalQuantity = dtos.Sum(x => x.Quantity),
                Subtotal = dtos.Sum(x => x.SalePrice * x.Quantity),
                TaxAmount = 0m, // Placeholder
                ShippingCost = 0m, // Placeholder
                TotalAmount = dtos.Sum(x => x.SalePrice * x.Quantity),
                SavingsAmount = dtos.Sum(x => (x.UnitPrice - x.SalePrice) * x.Quantity),
                HasUnavailableItems = dtos.Any(x => !x.IsAvailable),
                Items = dtos,
                UnavailableItems = dtos.Where(x => !x.IsAvailable)
            };
        }
    }
}