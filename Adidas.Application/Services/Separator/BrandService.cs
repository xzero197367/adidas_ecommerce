
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Separator
{

    public class BrandService : IBrandService//: GenericService<Brand, BrandResponseDto, CreateBrandDto, UpdateBrandDto>, IBrandService
    {
        private readonly IBrandRepository _brandRepository;

        public BrandService(
            IBrandRepository brandRepository
           )
        {
            _brandRepository = brandRepository;
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return Result.Failure("Brand not found.");

            await _brandRepository.SoftDeleteAsync(id);
            var result = await _brandRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to Create Brand.") : Result.Success();
        }

        public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
        {

            var brands = await _brandRepository.GetAllAsync();


            var activeBrands = brands.Where(b => b.IsActive && !b.IsDeleted);

            var brandDtos = activeBrands.Select(b => new BrandDto
            {

                Id = b.Id,
                UpdatedAt = b.UpdatedAt,
                IsActive = b.IsActive,

                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
            }).ToList();

            return brandDtos;
        }

        public async Task<Result> CreateAsync(CreateBrandDto createBrandDto)
        {

            if (await _brandRepository.GetBrandByNameAsync(createBrandDto.Name) != null)
            {
                return Result.Failure("A brand with this name already exists.");
            }


            var brand = new Brand
            {

                Name = createBrandDto.Name,
                Description = createBrandDto.Description,
                LogoUrl = createBrandDto.LogoUrl,
                IsActive = true,
                //CreatedAt = DateTime.UtcNow,
                //UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _brandRepository.AddAsync(brand);
                await _brandRepository.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating brand: {ex.Message}");
                return Result.Failure("An unexpected error occurred while creating the brand.");
            }
        }


        public async Task<Result> UpdateAsync(UpdateBrandDto dto)
        {
            var brandToUpdate = await _brandRepository.GetByIdAsync(dto.Id);

            if (brandToUpdate == null)
            {
                return Result.Failure("Brand not found.");
            }
            if (brandToUpdate.Name != dto.Name)
            {
                var brandWithSameName = await _brandRepository.GetBrandByNameAsync(dto.Name);
                if (brandWithSameName != null)
                {
                    return Result.Failure("A brand with this name already exists.");
                }
            }

            brandToUpdate.Name = dto.Name;
            brandToUpdate.Description = dto.Description;
            brandToUpdate.LogoUrl = dto.LogoUrl;
            brandToUpdate.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _brandRepository.UpdateAsync(brandToUpdate);
                await _brandRepository.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"An unexpected error occurred while updating the brand. {ex.Message}");
            }
        }

        public async Task<UpdateBrandDto> GetBrandToEditByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);

             if (brand == null)
             {
                return null;
             }
            var updateBrandDto = new UpdateBrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl
            };

            return updateBrandDto;
        }


        public async Task<BrandDto?> GetDetailsByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);

            if (brand == null)
            {
                return null;
            }

          var brandDto = new BrandDto
            {
                Id = brand.Id,
                IsActive = brand.IsActive,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
            };

            return brandDto;
        }

        public async Task<IEnumerable<BrandDto>> GetFilteredBrandsAsync(string statusFilter, string searchTerm)
        {
            var brands = await _brandRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "Active";
                brands = brands.Where(c => c.IsActive == isActive).ToList();
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                brands = brands.Where(c =>
                    c.Name != null && c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var brandDtos = brands.Select(b => new BrandDto
            {

                Id = b.Id,
                UpdatedAt = b.UpdatedAt,
                IsActive = b.IsActive,

                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
            }).ToList();

            return brandDtos;
        }

    }
}
 //#region Generic Service Overrides

    //protected override async Task ValidateCreateAsync(CreateBrandDto createDto)
    //{
    //    // Check if brand name already exists
    //    var existingBrand = await _brandRepository.GetBrandByNameAsync(createDto.Name);
    //    if (existingBrand != null)
    //    {
    //        throw new InvalidOperationException("Brand with this name already exists");
    //    }
    //}

    //protected override async Task ValidateUpdateAsync(Guid id, UpdateBrandDto updateDto)
    //{
    //    // Check if another brand with the same name exists
    //    var brandWithSameName = await _brandRepository.GetBrandByNameAsync(updateDto.Name);
    //    if (brandWithSameName != null && brandWithSameName.Id != id)
    //    {
    //        throw new InvalidOperationException("Another brand with this name already exists");
    //    }
    //}

    //protected override async Task BeforeCreateAsync(Brand entity)
    //{
    //    entity.CreatedAt = DateTime.UtcNow;
    //    entity.UpdatedAt = DateTime.UtcNow;
    //    entity.IsActive = true;
    //    entity.IsDeleted = false;
    //}

    //protected override async Task BeforeUpdateAsync(Brand entity)
    //{
    //    entity.UpdatedAt = DateTime.UtcNow;
    //}

    //#endregion

    //#region Brand-Specific Methods

    //public async Task<BrandResponseDto?> GetBrandByNameAsync(string name)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Getting brand by name: {BrandName}", name);
    //        var brand = await _brandRepository.GetBrandByNameAsync(name);

    //        if (brand == null)
    //        {
    //            _logger.LogWarning("Brand not found with name: {BrandName}", name);
    //            return null;
    //        }

    //        return _mapper.Map<BrandResponseDto>(brand);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving brand with name: {BrandName}", name);
    //        throw;
    //    }
    //}

    //public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
    //{
    //    try
    //    {
    //        _logger.LogInformation("Getting active brands");
    //        var brands = await _brandRepository.FindAsync(b => !b.IsDeleted && b.IsActive);
    //        return _mapper.Map<IEnumerable<BrandDto>>(brands);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving active brands");
    //        throw;
    //    }
    //}

    //public async Task<IEnumerable<BrandDto>> GetPopularBrandsAsync()
    //{
    //    try
    //    {
    //        _logger.LogInformation("Getting popular brands");
    //        var brands = await _brandRepository.GetPopularBrandsAsync();
    //        return _mapper.Map<IEnumerable<BrandDto>>(brands);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving popular brands");
    //        throw;
    //    }
    //}

    //public async Task<PagedResultDto<BrandDto>> GetPaginatedBrandListAsync(int pageNumber, int pageSize)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Getting paginated brand list - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
    //        var (brands, totalCount) = await _brandRepository.GetPagedAsync(pageNumber, pageSize, b => !b.IsDeleted);
    //        var brandList = _mapper.Map<IEnumerable<BrandDto>>(brands);

    //        return new PagedResultDto<BrandDto>
    //        {
    //            Items = brandList,
    //            TotalCount = totalCount,
    //            PageNumber = pageNumber,
    //            PageSize = pageSize,
    //            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving paginated brand list");
    //        throw;
    //    }
    //}

    //#endregion


 





