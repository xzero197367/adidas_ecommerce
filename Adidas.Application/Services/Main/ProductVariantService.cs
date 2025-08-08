using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Main
{
    public class ProductVariantService : GenericService<ProductVariant, ProductVariantDto, CreateProductVariantDto, UpdateProductVariantDto>, IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository; 
        private readonly IMapper _mapper;
        private readonly ILogger<ProductVariantService> _logger;

        public ProductVariantService(
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            IMapper mapper,
            ILogger<ProductVariantService> logger
        ) : base(productVariantRepository, mapper, logger)
        {
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ProductVariantDto?> GetBySkuAsync(string sku)
        {
            var variant = await _productVariantRepository.GetVariantBySkuAsync(sku);
            if (variant == null) return null;
            return _mapper.Map<ProductVariantDto>(variant);
        }



        public async Task<bool> AddImageAsync(Guid variantId, IFormFile imageFile)
        {
            try
            {
                var variant = await _productVariantRepository.GetByIdAsync(variantId);
                if (variant == null || imageFile == null || imageFile.Length == 0)
                    return false;

                var uploadsFolder = Path.Combine("wwwroot", "images", "variants");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var image = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = $"/images/variants/{uniqueFileName}",
                    VariantId = variantId,
                    ProductId = variant.ProductId,
                    IsPrimary = false,
                    SortOrder = 0,
                    AltText = "",
                    CreatedAt = DateTime.UtcNow,
                };

                await _productImageRepository.AddAsync(image);
                await _productImageRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to product variant {VariantId}", variantId);
                return false;
            }
        }
    }
}