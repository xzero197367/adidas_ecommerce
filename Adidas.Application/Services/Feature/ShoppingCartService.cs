using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;
using AutoMapper;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IWishlistRepository _wishlistRepository;
       private readonly IMapper _mapper;

        public ShoppingCartService(
            IShoppingCartRepository cartRepository,
            IWishlistRepository wishlistRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _wishlistRepository = wishlistRepository;
             _mapper = mapper;
        }

        public async Task<ShoppingCartItemDto> AddToCartAsync(ShoppingCartCreateDto addCreateDto)
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
           return _mapper.Map<ShoppingCartItemDto>(updatedItem);
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            return await _cartRepository.ClearCartAsync(userId);
        }

        public async Task<bool> MergeCartsAsync(string fromUserId, string toUserId)
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
            return await _cartRepository.ClearCartAsync(fromUserId);
        }

        public async Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsByUserIdAsync(string userId)
        {
            var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ShoppingCartItemDto>>(items);
        }

        public async Task<CartSummaryDto> GetCartSummaryAsync(string userId)
        {
            var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            var dtoItems = _mapper.Map<IEnumerable<ShoppingCartItemDto>>(items);

            var summary = new CartSummaryDto
            {
                UserId = userId,
                Items = dtoItems,
                ItemCount = dtoItems.Count(),
                TotalQuantity = dtoItems.Sum(i => i.Quantity),
                Subtotal = dtoItems.Sum(i => i.TotalPrice),
                SavingsAmount = dtoItems.Sum(i => i.UnitPrice - i.SalePrice),
                TotalAmount = dtoItems.Sum(i => i.TotalPrice),
                HasUnavailableItems = dtoItems.Any(i => !i.IsAvailable)
            };

            return summary;
        }

        public async Task<bool> MoveToWishlistAsync(string userId, Guid variantId)
        {
            var item = await _cartRepository.GetCartItemAsync(userId, variantId);
            if (item == null) return false;

            await _wishlistRepository.AddAsync(new Wishlist
            {
                UserId = userId,
                ProductId = item.Variant.ProductId,
                AddedAt = DateTime.UtcNow
            });

            return await _cartRepository.RemoveFromCartAsync(userId, variantId);
        }

        public async Task<bool> RemoveFromCartAsync(string userId, Guid variantId)
        {
            return await _cartRepository.RemoveFromCartAsync(userId, variantId);
        }

        public async Task<bool> RestoreSavedCartAsync(string userId)
        {
            // Custom logic needed if SaveCartForLaterAsync is implemented with a separate table
            return false;
        }

        public async Task<bool> SaveCartForLaterAsync(string userId)
        {
            // Custom logic needed if SaveCartForLaterAsync is implemented with a separate table
            return false;
        }

        public async Task<bool> ValidateCartItemsAsync(string userId)
        {
            var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            return items.All(i => i.Variant.StockQuantity >= i.Quantity);
        }

        public async Task<IEnumerable<ShoppingCartItemDto>> GetUnavailableItemsAsync(string userId)
        {
            var items = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            var unavailable = items.Where(i => i.Variant.StockQuantity < i.Quantity);
            return _mapper.Map<IEnumerable<ShoppingCartItemDto>>(unavailable);
        }

        public async Task<ShoppingCartItemDto> UpdateCartItemQuantityAsync(ShoppingChartUpdateDto shoppingChartUpdateDto)
        {
            var item = await _cartRepository.GetCartItemAsync(shoppingChartUpdateDto.UserId, shoppingChartUpdateDto.ProductVariantId);
            if (item == null) return null;

            item.Quantity = shoppingChartUpdateDto.Quantity;
            await _cartRepository.UpdateAsync(item);

            return _mapper.Map<ShoppingCartItemDto>(item);
        }

        public async Task<CartSummaryDto> GetCartSummaryWithTaxAsync(string userId, string? shippingAddress = null)
        {
            var summary = await GetCartSummaryAsync(userId);
            summary.TaxAmount = summary.Subtotal * 0.15m;
            summary.ShippingCost = 30m;
            summary.TotalAmount = summary.Subtotal + summary.TaxAmount + summary.ShippingCost;
            return summary;
        }

        //public Task<bool> MoveToWishlistAsync(Guid userId, Guid variantId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<bool> MoveFromWishlistAsync(Guid userId, Guid productId, Guid variantId)
        //{
        //    throw new NotImplementedException();
        //}
    }
} 