using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Separator
{
    public interface IBrandService : IGenericService<Brand, BrandResponseDto, CreateBrandDto, UpdateBrandDto>
    {
        // Brand-specific methods
        Task<BrandResponseDto?> GetBrandByNameAsync(string name);
        Task<IEnumerable<BrandListDto>> GetActiveBrandsAsync();
        Task<IEnumerable<BrandListDto>> GetPopularBrandsAsync();
        Task<PagedResultDto<BrandListDto>> GetPaginatedBrandListAsync(int pageNumber, int pageSize);
    }
}
