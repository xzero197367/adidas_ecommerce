using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IProductAttributeRepository : IGenericRepository<ProductAttribute>
    {
        Task<IEnumerable<ProductAttribute>> GetFilterableAttributesAsync();
        Task<IEnumerable<ProductAttribute>> GetRequiredAttributesAsync();
    }
}
