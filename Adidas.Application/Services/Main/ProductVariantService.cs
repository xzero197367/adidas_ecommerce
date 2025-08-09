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
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Main
{
    public class ProductVariantService : GenericService<ProductVariant, ProductVariantDto, CreateProductVariantDto, UpdateProductVariantDto>, IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository; 
        private readonly IMapper _mapper;
        private readonly ILogger<ProductVariantService> _logger;
        private readonly IHostingEnvironment _env;

        public ProductVariantService(
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            IMapper mapper,
            ILogger<ProductVariantService> logger,
                IHostingEnvironment  env

        ) : base(productVariantRepository, mapper, logger)
        {
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;
            _mapper = mapper;

            _logger = logger;
            _env = env;
        }

        public override async Task<ProductVariantDto> CreateAsync(CreateProductVariantDto createDto)
        {
            // تحقق هل يوجد نفس المنتج مع نفس الحجم واللون
            var existingVariant = await _productVariantRepository.FindAsync(v =>
                v.ProductId == createDto.ProductId &&
                v.Size.ToLower() == createDto.Size.ToLower() &&
                v.Color.ToLower() == createDto.Color.ToLower());

            if (existingVariant.Any())
            {
                throw new InvalidOperationException("A product variant with the same Product, Size, and Color already exists.");
            }

            var entity = _mapper.Map<ProductVariant>(createDto);

            if (string.IsNullOrWhiteSpace(entity.Sku))
            {
                entity.Sku = "SKU-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }

            // رفع الصورة وتعيين ImageUrl
            if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "variants");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{createDto.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createDto.ImageFile.CopyToAsync(stream);
                }

                entity.ImageUrl = $"/images/variants/{uniqueFileName}";
            }
            else
            {
                entity.ImageUrl = "/images/variants/1521f72e-6276-4166-98c6-d12d37314453_51JYkEpbhzL"; 
            }

            await BeforeCreateAsync(entity);

            var createdEntityEntry = await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            await AfterCreateAsync(createdEntityEntry.Entity);

            return _mapper.Map<ProductVariantDto>(createdEntityEntry.Entity);
        }

        private string GenerateSku(ProductVariant variant)
        {
            // مثال بسيط لتوليد SKU من بعض الخصائص - عدليه حسب متطلباتك
            return $"SKU-{variant.ProductId.ToString().Substring(0, 8)}-{variant.Color}-{variant.Size}";
        }



        public override async Task<IEnumerable<ProductVariantDto>> GetAllAsync()
        {
            var variants = await _productVariantRepository.GetAllAsync(v => v.Product, v => v.Images);
            return _mapper.Map<IEnumerable<ProductVariantDto>>(variants);
        }


        public async Task<bool> AddImageAsync(Guid variantId, IFormFile imageFile)
        {
            var variant = await _productVariantRepository.GetByIdAsync(variantId);
            if (variant == null || imageFile == null || imageFile.Length == 0)
                return false;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "variants");
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

        public async Task<ProductVariantDto?> GetBySkuAsync(string sku)
        {
            var variant = await _productVariantRepository.GetVariantBySkuAsync(sku);
            if (variant == null) return null;
            return _mapper.Map<ProductVariantDto>(variant);
        }



        public override async Task<ProductVariantDto> UpdateAsync(Guid id, UpdateProductVariantDto updateDto)
        {
            try
            {
                var existingEntity = await _productVariantRepository.GetByIdAsync(id);
                if (existingEntity == null)
                    throw new KeyNotFoundException($"Product variant with id {id} not found");

                var duplicates = await _productVariantRepository.FindAsync(v =>
                    v.ProductId == updateDto.ProductId &&
                    v.Size.ToLower() == updateDto.Size.ToLower() &&
                    v.Color.ToLower() == updateDto.Color.ToLower() &&
                    v.Id != id);

                if (duplicates.Any())
                {
                    throw new InvalidOperationException("A product variant with the same Product, Size, and Color already exists.");
                }

                if (updateDto.ImageFile != null && updateDto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "variants");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{updateDto.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.ImageFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingEntity.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_env.WebRootPath, existingEntity.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(oldImagePath))
                            File.Delete(oldImagePath);
                    }

                    existingEntity.ImageUrl = $"/images/variants/{uniqueFileName}";
                }

                _mapper.Map(updateDto, existingEntity);

                await BeforeUpdateAsync(existingEntity);

                var updatedEntry = await _productVariantRepository.UpdateAsync(existingEntity);
                await _productVariantRepository.SaveChangesAsync();

                await AfterUpdateAsync(updatedEntry.Entity);

                return _mapper.Map<ProductVariantDto>(updatedEntry.Entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product variant with id {Id}", id);
                throw;
            }
        }


    }
}