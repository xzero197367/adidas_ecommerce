
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IProductAttributeValueRepository: IGenericRepository<ProductAttributeValue>
    {
        Task<IEnumerable<ProductAttributeValue>> GetValuesByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductAttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId);
        Task<ProductAttributeValue?> GetValueAsync(Guid valueId);
    }
}
