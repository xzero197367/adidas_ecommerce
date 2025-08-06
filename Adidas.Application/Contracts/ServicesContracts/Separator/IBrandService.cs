using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.Models.Separator;


namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface IBrandService : IGenericService<Brand, BrandResponseDto, BrandCreateDto, BrandUpdateDto>
    {
        // Brand-specific methods
        Task<BrandResponseDto?> GetBrandByNameAsync(string name);
        Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();
        Task<IEnumerable<BrandDto>> GetPopularBrandsAsync();
        Task<PagedResultDto<BrandDto>> GetPaginatedBrandListAsync(int pageNumber, int pageSize);
    }
}
