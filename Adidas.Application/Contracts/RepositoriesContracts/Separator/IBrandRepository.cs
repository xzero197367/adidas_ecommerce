using Adidas.Models.Separator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Separator
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<Brand?> GetBrandByNameAsync(string name);
        Task<IEnumerable<Brand>> GetPopularBrandsAsync();
    }
}
