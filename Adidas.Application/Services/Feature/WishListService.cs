using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.WishLIstDTOS;
using System.Text;
using Adidas.DTOs.Common_DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class WishListService : IWishListService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ILogger logger;

        public WishListService(IWishlistRepository wishListRepository, ILogger logger)
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
            }catch (Exception ex)
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
            }catch (Exception ex)
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
            }catch (Exception ex)
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
            }catch (Exception ex)
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
            }catch (Exception ex)
            {
                logger.LogError(ex, "Error getting wishlist summary: {UserId}", userId);
                return OperationResult<IEnumerable<WishlistDto>>.Fail(ex.Message);
            }
        }

    

        // public Task<OperationResult<string>> GenerateWishlistShareLinkAsync(string userId)
        // {
        //     // Generate token and persist or encode the userId
        //     return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString())));
        // }

        // public Task<OperationResult<IEnumerable<WishlistDto>>> GetSharedWishlistAsync(string shareToken)
        // {
        //     var userId = new String(Encoding.UTF8.GetString(Convert.FromBase64String(shareToken)));
        //     return GetWishlistByUserIdAsync(userId);
        // }
        //
        // public Task<OperationResult<bool>> NotifyWhenInStockAsync(string userId, Guid productId)
        // {
        //     // This might update a flag in Wishlist table for notification
        //     throw new NotImplementedException("Depends on stock notification tracking.");
        // }
        //
        // public Task<OperationResult<bool>> RemoveStockNotificationAsync(string userId, Guid productId)
        // {
        //     // Update NotifyWhenInStock = false in Wishlist
        //     // throw new NotImplementedException("Depends on stock notification tracking.");
        // }
    }
}