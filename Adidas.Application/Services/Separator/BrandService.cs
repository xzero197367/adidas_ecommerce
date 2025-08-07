using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Separator
{
    public class BrandService : GenericService<Brand, BrandDto, BrandCreateDto, BrandUpdateDto>, IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<BrandService> _logger;

        public BrandService(
            IBrandRepository brandRepository,
            ILogger<BrandService> logger
        ) : base(brandRepository, logger)
        {
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<OperationResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);
                if (brand == null)
                {
                    return OperationResult<bool>.Fail("Brand not found.");
                }

                await _brandRepository.SoftDeleteAsync(id);
                var result = await _brandRepository.SaveChangesAsync();

                if (result == null)
                {
                    return OperationResult<bool>.Fail("Failed to Delete brand.");
                }

                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<BrandDto>>> GetActiveBrandsAsync()
        {
            try
            {
                var activeBrands = await _brandRepository.GetAll().Where(b => b.IsActive && !b.IsDeleted).ToListAsync();
                return OperationResult<IEnumerable<BrandDto>>.Success(activeBrands.Adapt<IEnumerable<BrandDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active brands");
                return OperationResult<IEnumerable<BrandDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> CreateAsync(BrandCreateDto createBrandDto)
        {
            try
            {
                if (await _brandRepository.GetBrandByNameAsync(createBrandDto.Name) != null)
                {
                    return OperationResult<bool>.Fail("A brand with this name already exists.");
                }

                var createdBrand = await _brandRepository.AddAsync(createBrandDto.Adapt<Brand>());
                await _brandRepository.SaveChangesAsync();
                createdBrand.State = EntityState.Detached;

                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }


        public async Task<OperationResult<bool>> UpdateAsync(BrandUpdateDto dto)
        {
            try
            {
                var brandToUpdate = await _brandRepository.GetByIdAsync(dto.Id);

                if (brandToUpdate == null)
                {
                    return OperationResult<bool>.Fail("Brand not found.");
                }

                if (brandToUpdate.Name != dto.Name)
                {
                    var brandWithSameName = await _brandRepository.GetBrandByNameAsync(dto.Name);
                    if (brandWithSameName != null)
                    {
                        return OperationResult<bool>.Fail("A brand with this name already exists.");
                    }
                }

                brandToUpdate.Name = dto.Name;
                brandToUpdate.Description = dto.Description;
                brandToUpdate.LogoUrl = dto.LogoUrl;
                brandToUpdate.UpdatedAt = DateTime.UtcNow;

                var result = await _brandRepository.UpdateAsync(brandToUpdate);
                await _brandRepository.SaveChangesAsync();
                result.State = EntityState.Detached;
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<BrandUpdateDto>> GetBrandToEditByIdAsync(Guid id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);

                if (brand == null)
                {
                    return OperationResult<BrandUpdateDto>.Fail("Brand not found.");
                }

                return OperationResult<BrandUpdateDto>.Success(brand.Adapt<BrandUpdateDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand to edit");
                return OperationResult<BrandUpdateDto>.Fail(ex.Message);
            }
        }


        public async Task<OperationResult<BrandDto>> GetDetailsByIdAsync(Guid id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);

                if (brand == null)
                {
                    return OperationResult<BrandDto>.Fail("Brand not found.");
                }

                return OperationResult<BrandDto>.Success(brand.Adapt<BrandDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand details");
                return OperationResult<BrandDto>.Fail(ex.Message);
            }
        }
    }
}