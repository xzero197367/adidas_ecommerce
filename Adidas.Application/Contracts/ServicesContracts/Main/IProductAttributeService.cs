using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductAttributeService : IGenericService<ProductAttribute, ProductAttributeDto, ProductAttributeCreateDto, ProductAttributeUpdateDto>
    {
        Task<IEnumerable<ProductAttribute>> GetFilterableAttributesAsync();
        Task<IEnumerable<ProductAttribute>> GetRequiredAttributesAsync();
    }
}
