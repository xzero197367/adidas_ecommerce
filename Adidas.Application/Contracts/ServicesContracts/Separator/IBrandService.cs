using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface IBrandService 
    {
        Task<Result> DeleteAsync(Guid id);
        Task<Result> CreateAsync(BrandCreateDto createBrandDto);
        Task<Result> UpdateAsync(BrandUpdateDto dto);
        Task<BrandUpdateDto> GetBrandToEditByIdAsync(Guid id);      
        Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();
        Task<BrandDto?> GetDetailsByIdAsync(Guid id);
        Task<IEnumerable<BrandDto>> GetFilteredBrandsAsync(string statusFilter, string searchTerm);
        Task<Result> ToggleBrandStatusAsync(Guid categoryId);         
    }
}
