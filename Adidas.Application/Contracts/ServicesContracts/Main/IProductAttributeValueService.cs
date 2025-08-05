using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductAttributeValueService : IGenericService<ProductAttributeValue, ProductAttributeValueDto, ProductAttributeValueCreateDto, ProductAttributeValueUpdateDto>
    {
        
        Task<IEnumerable<ProductAttributeValue>> GetValuesByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductAttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId);
        Task<ProductAttributeValue?> GetValueAsync(Guid valueId);
    }
}
