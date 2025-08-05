using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Main
{
    public class ProductImageService : GenericService<ProductImage, ProductImageDto, ProductImageCreateDto, UpdateProductImageDto>, IProductImageService
    {
        private readonly IProductAttrbuteRepository _productImageRepository;
        public ProductImageService(
            IProductAttrbuteRepository productImageRepository,
            IMapper mapper,
            ILogger<ProductImageService> logger)
            : base(productImageRepository, mapper, logger)
        {
            _productImageRepository = productImageRepository;
        }
        public async Task<IEnumerable<ProductImageDto>> GetImagesByProductIdAsync(Guid productId)
        {
            var images = await _productImageRepository.GetImagesByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ProductImageDto>>(images);
        }
        public async Task<IEnumerable<ProductImageDto>> GetImagesByVariantIdAsync(Guid variantId)
        {
            var images = await _productImageRepository.GetImagesByVariantIdAsync(variantId);
            return _mapper.Map<IEnumerable<ProductImageDto>>(images);
        }

        // Get primary image for a product - returns entity
        public async Task<ProductImage?> GetPrimaryImageAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting primary image for product {ProductId}", productId);

                var primaryImage = await _productImageRepository.GetPrimaryImageAsync(productId);

                if (primaryImage != null)
                {
                    _logger.LogInformation("Found primary image {ImageId} for product {ProductId}", primaryImage.Id, productId);
                }
                else
                {
                    _logger.LogWarning("No primary image found for product {ProductId}", productId);
                }

                return primaryImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary image for product {ProductId}", productId);
                throw;
            }
        }

        // Explicit interface implementation - returns entities
        Task<IEnumerable<ProductImage>> IProductImageService.GetImagesByProductIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting product images (entities) for product {ProductId}", productId);
                return _productImageRepository.GetImagesByProductIdAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images (entities) for product {ProductId}", productId);
                throw;
            }
        }

        // Explicit interface implementation - returns entities
        Task<IEnumerable<ProductImage>> IProductImageService.GetImagesByVariantIdAsync(Guid variantId)
        {
            try
            {
                _logger.LogInformation("Getting variant images (entities) for variant {VariantId}", variantId);
                return _productImageRepository.GetImagesByVariantIdAsync(variantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant images (entities) for variant {VariantId}", variantId);
                throw;
            }
        }
    }
}
