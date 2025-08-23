using Adidas.DTOs.Feature.WishLIstDTOS;
using Adidas.DTOs.CommonDTOs;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IWishListService: IGenericService<Wishlist, WishlistDto, WishlistCreateDto, WishlistUpdateDto>
    {
        Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistByUserIdAsync(string userId);
        Task<OperationResult<WishlistDto>> AddToWishlistAsync(WishlistCreateDto addDto);
        Task<OperationResult<bool>> RemoveFromWishlistAsync(string userId, Guid productId);
        Task<OperationResult<bool>> IsProductInWishlistAsync(string userId, Guid productId);
        Task<OperationResult<int>> GetWishlistCountAsync(string userId);
        Task<OperationResult<IEnumerable<WishlistDto>>> GetWishlistSummaryAsync(string userId);
       
    }
}
