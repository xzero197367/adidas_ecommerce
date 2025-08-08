
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductImageService : IGenericService<ProductImage, ProductImageDto, ProductImageCreateDto, ProductImageUpdateDto>
    {
        Task<OperationResult<IEnumerable<ProductImageDto>>> GetImagesByProductIdAsync(Guid productId);
        Task<OperationResult<IEnumerable<ProductImageDto>>> GetImagesByVariantIdAsync(Guid variantId);
        Task<OperationResult<ProductImageDto>> GetPrimaryImageAsync(Guid productId);
    }
}
