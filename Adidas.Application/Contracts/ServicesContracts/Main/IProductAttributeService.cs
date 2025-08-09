using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductAttributeService : IGenericService<ProductAttribute, ProductAttributeDto, ProductAttributeCreateDto, ProductAttributeUpdateDto>
    {
        Task<OperationResult<IEnumerable<ProductAttributeDto>>> GetFilterableAttributesAsync();
        Task<OperationResult<IEnumerable<ProductAttributeDto>>> GetRequiredAttributesAsync();
    }
}
