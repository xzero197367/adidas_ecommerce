using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductAttributeValueService : IGenericService<ProductAttributeValue, ProductAttributeValueDto,
        ProductAttributeValueCreateDto, ProductAttributeValueUpdateDto>
    {
        Task<OperationResult<ProductAttributeValueDto>> CreateAsync(
            ProductAttributeValueCreateDto productAttributeValueCreateDto);

        Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> CreateRangeAsync(
            IEnumerable<ProductAttributeValueCreateDto> createDtos);

        Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> GetValuesByProductIdAsync(Guid productId);
        Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> GetValuesByAttributeIdAsync(Guid attributeId);
        Task<OperationResult<ProductAttributeValueDto>> GetValueAsync(Guid valueId);
    }
}