using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Application.Services.Main
{
    public class ProductVariantService :
        GenericService<ProductVariant, ProductVariantDto, ProductVariantCreateDto, ProductVariantUpdateDto>,
        IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository;

        private readonly ILogger<ProductVariantService> _logger;
        private readonly IHostingEnvironment _env;

        public ProductVariantService(
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            ILogger<ProductVariantService> logger,
            IHostingEnvironment env
        ) : base(productVariantRepository, logger)
        {
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;

            _logger = logger;
            _env = env;
        }

        public override async Task<OperationResult<ProductVariantDto>> CreateAsync(ProductVariantCreateDto createDto)
        {
            try
            {
                // تحقق هل يوجد نفس المنتج مع نفس الحجم واللون
                var existingVariant = await _productVariantRepository.GetAll().Where(v =>
                    v.ProductId == createDto.ProductId &&
                    v.Size.ToLower() == createDto.Size.ToLower() &&
                    v.Color.ToLower() == createDto.Color.ToLower()).ToListAsync();

                if (existingVariant.Any())
                {
                    return OperationResult<ProductVariantDto>.Fail(
                        "A variant with the same size and color already exists for this product.");
                }

                var entity = createDto.Adapt<ProductVariant>();

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

                return OperationResult<ProductVariantDto>.Success(createdEntityEntry.Entity.Adapt<ProductVariantDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a product variant.");
                return OperationResult<ProductVariantDto>.Fail("An error occurred while creating a product variant.");
            }
        }

        private string GenerateSku(ProductVariant variant)
        {
            // مثال بسيط لتوليد SKU من بعض الخصائص - عدليه حسب متطلباتك
            return $"SKU-{variant.ProductId.ToString().Substring(0, 8)}-{variant.Color}-{variant.Size}";
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
            return variant.Adapt<ProductVariantDto>();
        }


        public async Task<OperationResult<ProductVariantDto>> UpdateAsync(Guid id,
            ProductVariantUpdateDto productVariantUpdateDto)
        {
            try
            {
                var existingEntity = await _productVariantRepository.GetByIdAsync(id);
                if (existingEntity == null)
                    throw new KeyNotFoundException($"Product variant with id {id} not found");

                //var duplicates = await _productVariantRepository.GetAll(q => q.Where(v =>
                //    v.ProductId == productVariantUpdateDto.ProductId &&
                //    v.Size.ToLower() == productVariantUpdateDto.Size.ToLower() &&
                //    v.Color.ToLower() == productVariantUpdateDto.Color.ToLower() &&
                //    v.Id != id)).ToListAsync();
                var duplicates = await  _productVariantRepository.GetAll().Where(v =>
                    v.ProductId == productVariantUpdateDto.ProductId &&
                    v.Size.ToLower() == productVariantUpdateDto.Size.ToLower() &&
                    v.Color.ToLower() == productVariantUpdateDto.Color.ToLower() &&
                    v.Id != id).ToListAsync();



                if (duplicates.Any())
                {
                    throw new InvalidOperationException(
                        "A product variant with the same Product, Size, and Color already exists.");
                }

                if (productVariantUpdateDto.ImageFile != null && productVariantUpdateDto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "variants");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{productVariantUpdateDto.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productVariantUpdateDto.ImageFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingEntity.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_env.WebRootPath,
                            existingEntity.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(oldImagePath))
                            File.Delete(oldImagePath);
                    }

                    existingEntity.ImageUrl = $"/images/variants/{uniqueFileName}";
                }

                existingEntity = productVariantUpdateDto.Adapt<ProductVariant>();

                await BeforeUpdateAsync(existingEntity);

                var updatedEntry = await _productVariantRepository.UpdateAsync(existingEntity);
                await _productVariantRepository.SaveChangesAsync();

                await AfterUpdateAsync(updatedEntry.Entity);

                return OperationResult<ProductVariantDto>.Success(updatedEntry.Entity.Adapt<ProductVariantDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product variant with id {Id}", id);
                return OperationResult<ProductVariantDto>.Fail("Error updating product variant: " + ex.Message);
            }
        }
    }
}