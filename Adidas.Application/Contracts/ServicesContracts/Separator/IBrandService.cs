using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface IBrandService //: IGenericService<Brand, BrandResponseDto, CreateBrandDto, UpdateBrandDto>
    {
        Task<Result> DeleteAsync(Guid id);
        Task<Result> CreateAsync(CreateBrandDto createBrandDto);
        Task<Result> UpdateAsync(UpdateBrandDto dto);
        Task<UpdateBrandDto> GetBrandToEditByIdAsync(Guid id);

        // Brand-specific methods
        //Task<BrandResponseDto?> GetBrandByNameAsync(string name);
        Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();
        Task<BrandDto> GetDetailsByIdAsync(Guid id);
        Task<IEnumerable<BrandDto>> GetFilteredBrandsAsync(string statusFilter, string searchTerm);
        Task<Result> ToggleCategoryStatusAsync(Guid categoryId);

        //Task<IEnumerable<BrandDto>> GetPopularBrandsAsync();
        //Task<PagedResultDto<BrandDto>> GetPaginatedBrandListAsync(int pageNumber, int pageSize);
    }
}
