using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.WishLIstDTOS;
using System.Text;
using AutoMapper;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class WishListService : IWishListService
    {

        private readonly IWishlistRepository _wishlistRepository;
        private readonly IMapper _mapper;

        public WishListService(IWishlistRepository wishListRepository, IMapper mapper)
        {
            _wishlistRepository = wishListRepository;
            _mapper = mapper;
        }
     
        //public WishlistService(IWishListRepository wishlistRepository)//, IMapper mapper)
        //{
        //    _wishlistRepository = wishlistRepository;
        //  //  _mapper = mapper;
        //}

        public async Task<IEnumerable<WishlistDto>> GetWishlistByUserIdAsync(string userId)
        {
            var wishlists = await _wishlistRepository.GetWishlistByUserIdAsync(userId);
            return wishlists.Select(w => _mapper.Map<WishlistDto>(w));
        }

        public async Task<WishlistDto> AddToWishlistAsync(AddToWishlistDto addDto)
        {
            var exists = await _wishlistRepository.IsProductInWishlistAsync(addDto.UserId, addDto.ProductId);
            if (exists) throw new Exception("Product already in wishlist.");

            var wishlist = new Wishlist
            {
                UserId = addDto.UserId,
                ProductId = addDto.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            await _wishlistRepository.AddAsync(wishlist);
            return _mapper.Map<WishlistDto>(wishlist);
        }

        public Task<bool> RemoveFromWishlistAsync(string userId, Guid productId) =>
            _wishlistRepository.RemoveFromWishlistAsync(userId, productId);

        public Task<bool> IsProductInWishlistAsync(string userId, Guid productId) =>
            _wishlistRepository.IsProductInWishlistAsync(userId, productId);

        public Task<int> GetWishlistCountAsync(string userId) =>
            _wishlistRepository.GetWishlistCountAsync(userId);

        public async Task<IEnumerable<WishlistDto>> GetWishlistSummaryAsync(string userId)
        {
            var wishlists = await _wishlistRepository.GetWishlistByUserIdAsync(userId);
            return wishlists.Select(w =>
            {
                var dto = _mapper.Map<WishlistDto>(w);
                dto.SavingsAmount = dto.IsOnSale ? (dto.Price - dto.SalePrice) : null;
                return dto;
            });
        }

        public Task<bool> MoveToCartAsync(string userId, Guid productId, Guid variantId)
        {
            // You would call CartService.AddToCartAsync internally here
            throw new NotImplementedException("Integrate with CartService.");
        }

        public Task<string> GenerateWishlistShareLinkAsync(string userId)
        {
            // Generate token and persist or encode the userId
            return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString())));
        }

        public Task<IEnumerable<WishlistDto>> GetSharedWishlistAsync(string shareToken)
        {
            var userId = new String(Encoding.UTF8.GetString(Convert.FromBase64String(shareToken)));
            return GetWishlistByUserIdAsync(userId);
        }

        public Task<bool> NotifyWhenInStockAsync(string userId, Guid productId)
        {
            // This might update a flag in Wishlist table for notification
            throw new NotImplementedException("Depends on stock notification tracking.");
        }

        public Task<bool> RemoveStockNotificationAsync(string userId, Guid productId)
        {
            // Update NotifyWhenInStock = false in Wishlist
            throw new NotImplementedException("Depends on stock notification tracking.");
        }
    }
}