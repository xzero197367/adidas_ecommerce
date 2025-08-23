using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.WishLIstDTOS;
using System.Text;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class WishListService : GenericService<Wishlist, WishlistDto, WishlistCreateDto, WishlistUpdateDto>,
        IWishListService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ILogger<WishListService> logger;

        public WishListService(IWishlistRepository wishListRepository, ILogger<WishListService> logger) : base(wishListRepository,
            logger)
        {
            _wishlistRepository = wishListRepository;
            this.logger = logger;
        }


        public async Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistByUserIdAsync(string userId)
        {
            try
            {
                var wishlists = await _wishlistRepository.GetWishlistByUserIdAsync(userId);
                return OperationResult<IEnumerable<WishlistDto>>.Success(
                    wishlists.Adapt<IEnumerable<WishlistDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting wishlist by user id: {UserId}", userId);
                return OperationResult<IEnumerable<WishlistDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<WishlistDto>> AddToWishlistAsync(WishlistCreateDto addDto)
        {
            try
            {
                var exists = await _wishlistRepository.IsProductInWishlistAsync(addDto.UserId, addDto.ProductId);
                if (exists) return OperationResult<WishlistDto>.Fail("Product already exists in wishlist.");

                var wishlist = await _wishlistRepository.AddAsync(addDto.Adapt<Wishlist>());
                await _wishlistRepository.SaveChangesAsync();
                wishlist.State = EntityState.Detached;
                return OperationResult<WishlistDto>.Success(wishlist.Entity.Adapt<WishlistDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding product to wishlist: {ProductId}", addDto.ProductId);
                return OperationResult<WishlistDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> RemoveFromWishlistAsync(string userId, Guid productId)
        {
            try
            {
                var result = await _wishlistRepository.RemoveFromWishlistAsync(userId, productId);
                await _wishlistRepository.SaveChangesAsync();

                return OperationResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing product from wishlist: {ProductId}", productId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> IsProductInWishlistAsync(string userId, Guid productId)
        {
            try
            {
                var result = await _wishlistRepository.IsProductInWishlistAsync(userId, productId);
                return OperationResult<bool>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking if product is in wishlist: {ProductId}", productId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<int>> GetWishlistCountAsync(string userId)
        {
            try
            {
                var count = await _wishlistRepository.GetWishlistCountAsync(userId);
                return OperationResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting wishlist count: {UserId}", userId);
                return OperationResult<int>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistSummaryAsync(string userId)
        {
            try
            {
                var wishlists = await _wishlistRepository.GetWishlistByUserIdAsync(userId);
                return OperationResult<IEnumerable<WishlistDto>>.Success(
                    wishlists.Adapt<IEnumerable<WishlistDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting wishlist summary: {UserId}", userId);
                return OperationResult<IEnumerable<WishlistDto>>.Fail(ex.Message);
            }
        }


      
    }
}