
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;
using Mapster;
using Microsoft.EntityFrameworkCore;

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
        
        public virtual async Task<OperationResult<IEnumerable<BrandDto>>> GetAllAsync(
            Func<IQueryable<Brand>, IQueryable<Brand>>? queryFunc = null)
        {
            try
            {
                var entities = await _brandRepository.GetAll(queryFunc).ToListAsync();
                return OperationResult<IEnumerable<BrandDto>>.Success(entities.Adapt<IEnumerable<BrandDto>>());
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<BrandDto>>.Fail("Error getting all entities: " + ex.Message);
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id,
                  c => c.Products
                );
            if (brand == null)
                return Result.Failure("Brand not found.");
            if (brand.Products.Count() != 0)
                return Result.Failure("Cannot delete a brand that has products.");

            await _brandRepository.HardDeleteAsync(id);
            var result = await _brandRepository.SaveChangesAsync();

            return result == null ? Result.Failure("Failed to Delete Brand.") : Result.Success();
        }

        public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
        {

            var brands = _brandRepository.GetAll();


            var activeBrands = await brands.Where(b => b.IsActive && !b.IsDeleted).ToListAsync();

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

        public async Task<Result> CreateAsync(BrandCreateDto createBrandDto)
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


        public async Task<Result> UpdateAsync(BrandUpdateDto dto)
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

        public async Task<BrandUpdateDto> GetBrandToEditByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);

            if (brand == null)
            {
                return null;
            }
            var updateBrandDto = new BrandUpdateDto
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
            var brand = await _brandRepository.GetByIdAsync(id, b => b.Products);

            if (brand == null)
                return null;

            return new BrandDto
            {
                Id = brand.Id,
                IsActive = brand.IsActive,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Products = brand.Products?.Select(product => new ProductDto
                {
                    Id = product.Id,
                    IsActive = product.IsActive,
                    Name = product.Name,
                    Description = product.Description,
                    ShortDescription = product.ShortDescription,
                    Sku = product.Sku,
                    Price = product.Price,
                    SalePrice = product.SalePrice,
                    GenderTarget = product.GenderTarget,
                    MetaDescription = product.MetaDescription
                }).ToList() ?? new List<ProductDto>()
            };
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


        public async Task<Result> ToggleBrandStatusAsync(Guid brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null)
                return Result.Failure("brand not found.");

            brand.IsActive = !brand.IsActive;

            try
            {
                await _brandRepository.UpdateAsync(brand);
                var result = await _brandRepository.SaveChangesAsync();

                if (result > 0)
                    return Result.Success();
                else
                    return Result.Failure("No changes were made to the database.");
            }
            catch (Exception ex)
            {

                return Result.Failure("An error occurred while updating the brand status.");
            }
        }
        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            var brands = await _brandRepository.GetAll().ToListAsync();
            return brands.Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive,
                UpdatedAt = b.UpdatedAt
            });
        }


    }
}





