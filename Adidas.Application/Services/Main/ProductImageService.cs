
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Main
{
    public class ProductImageService :
        GenericService<ProductImage, ProductImageDto, ProductImageCreateDto, ProductImageUpdateDto>,
        IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly ILogger<ProductImageService> _logger;

        public ProductImageService(
            IProductImageRepository productImageRepository,
            ILogger<ProductImageService> logger) : base(productImageRepository, logger)
        {
            _productImageRepository = productImageRepository;
            _logger = logger;
        }

        public async Task<OperationResult<IEnumerable<ProductImageDto>>> GetImagesByProductIdAsync(Guid productId)
        {
            try
            {
                var images = await _productImageRepository.GetImagesByProductIdAsync(productId);

                return OperationResult<IEnumerable<ProductImageDto>>.Success(
                    images.Adapt<IEnumerable<ProductImageDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images");
                return OperationResult<IEnumerable<ProductImageDto>>.Fail(ex.Message);
            }
        }


        // Get primary image for a product - returns entity
        public async Task<OperationResult<ProductImageDto>> GetPrimaryImageAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting primary image for product {ProductId}", productId);

                var primaryImage = await _productImageRepository.GetPrimaryImageAsync(productId);

                if (primaryImage != null)
                {
                    _logger.LogInformation("Found primary image {ImageId} for product {ProductId}", primaryImage.Id,
                        productId);
                    return OperationResult<ProductImageDto>.Success(primaryImage.Adapt<ProductImageDto>());
                }
                else
                {
                    _logger.LogWarning("No primary image found for product {ProductId}", productId);
                    return OperationResult<ProductImageDto>.Fail("No primary image found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary image for product {ProductId}", productId);
                return OperationResult<ProductImageDto>.Fail(ex.Message);
            }
        }

        // Explicit interface implementation - returns entities


        // Explicit interface implementation - returns entities
        public async Task<OperationResult<IEnumerable<ProductImageDto>>> GetImagesByVariantIdAsync(Guid variantId)
        {
            try
            {
                // _logger.LogInformation("Getting variant images (entities) for variant {VariantId}", variantId);
                var result = await _productImageRepository.GetImagesByVariantIdAsync(variantId);

                return OperationResult<IEnumerable<ProductImageDto>>.Success(
                    result.Adapt<IEnumerable<ProductImageDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant images (entities) for variant {VariantId}", variantId);
                return OperationResult<IEnumerable<ProductImageDto>>.Fail(ex.Message);
            }
        }
    }
}