using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface IBrandService : IGenericService<Brand, BrandDto, BrandCreateDto, BrandUpdateDto>
    {
        Task<OperationResult<bool>> DeleteAsync(Guid id);
        Task<OperationResult<bool>> CreateAsync(BrandCreateDto createBrandDto);
        Task<OperationResult<bool>> UpdateAsync(BrandUpdateDto dto);
        Task<OperationResult<BrandUpdateDto>> GetBrandToEditByIdAsync(Guid id);

        // Brand-specific methods
        //Task<BrandResponseDto?> GetBrandByNameAsync(string name);
        Task<OperationResult<IEnumerable<BrandDto>>> GetActiveBrandsAsync();
        Task<OperationResult<BrandDto>> GetDetailsByIdAsync(Guid id);
        //Task<IEnumerable<BrandDto>> GetPopularBrandsAsync();
        //Task<PagedResultDto<BrandDto>> GetPaginatedBrandListAsync(int pageNumber, int pageSize);
    }
}
