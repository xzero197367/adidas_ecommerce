using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Services.Feature;
using Adidas.DTOs.Feature.WishLIstDTOS;
using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Feature
{
    public class WishListService : IWishListService
    {

        private readonly IWishListRepository _wishlistRepository;
        // private readonly IMapper _mapper;

        public WishListService()
        {
            
        }
        //public WishlistService(IWishListRepository wishlistRepository)//, IMapper mapper)
        //{
        //    _wishlistRepository = wishlistRepository;
        //  //  _mapper = mapper;
        //}

        public async Task<IEnumerable<WishlistItemDto>> GetWishlistByUserIdAsync(Guid userId)
        {
            var wishlists = await _wishlistRepository.GetWishListByUserIdAsync(userId);
            return wishlists.Select(w => _mapper.Map<WishlistItemDto>(w));
        }

        public async Task<WishlistItemDto> AddToWishlistAsync(AddToWishlistDto addDto)
        {
            var exists = await _wishlistRepository.IsProductInWishListAsync(addDto.UserId, addDto.ProductId);
            if (exists) throw new Exception("Product already in wishlist.");

            var wishlist = new Wishlist
            {
                UserId = addDto.UserId,
                ProductId = addDto.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            await _wishlistRepository.AddAsync(wishlist);
            return _mapper.Map<WishlistItemDto>(wishlist);
        }

        public Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId) =>
            _wishlistRepository.RemoveFromWishListAsync(userId, productId);

        public Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId) =>
            _wishlistRepository.IsProductInWishListAsync(userId, productId);

        public Task<int> GetWishlistCountAsync(Guid userId) =>
            _wishlistRepository.GetWishListCountAsync(userId);

        public async Task<IEnumerable<WishlistItemDto>> GetWishlistSummaryAsync(Guid userId)
        {
            var wishlists = await _wishlistRepository.GetWishListByUserIdAsync(userId);
            return wishlists.Select(w =>
            {
                var dto = _mapper.Map<WishlistItemDto>(w);
                dto.SavingsAmount = dto.IsOnSale ? (dto.Price - dto.SalePrice) : null;
                return dto;
            });
        }

        public Task<bool> MoveToCartAsync(Guid userId, Guid productId, Guid variantId)
        {
            // You would call CartService.AddToCartAsync internally here
            throw new NotImplementedException("Integrate with CartService.");
        }

        public Task<string> GenerateWishlistShareLinkAsync(Guid userId)
        {
            // Generate token and persist or encode the userId
            return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString())));
        }

        public Task<IEnumerable<WishlistItemDto>> GetSharedWishlistAsync(string shareToken)
        {
            var userId = new Guid(Encoding.UTF8.GetString(Convert.FromBase64String(shareToken)));
            return GetWishlistByUserIdAsync(userId);
        }

        public Task<bool> NotifyWhenInStockAsync(Guid userId, Guid productId)
        {
            // This might update a flag in Wishlist table for notification
            throw new NotImplementedException("Depends on stock notification tracking.");
        }

        public Task<bool> RemoveStockNotificationAsync(Guid userId, Guid productId)
        {
            // Update NotifyWhenInStock = false in Wishlist
            throw new NotImplementedException("Depends on stock notification tracking.");
        }
    }
}