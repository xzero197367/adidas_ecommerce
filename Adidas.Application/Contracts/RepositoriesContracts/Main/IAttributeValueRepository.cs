using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IAttributeValueRepository: IGenericRepository<ProductAttributeValue>
    {
        Task<IEnumerable<ProductAttributeValue>> GetValuesByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductAttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId);
        Task<ProductAttributeValue?> GetValueAsync(Guid valueId);
    }
}
