using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Main
{
    public class ProductAttributeService :
        GenericService<ProductAttribute, ProductAttributeDto, ProductAttributeCreateDto, ProductAttributeUpdateDto>,
        IProductAttributeService
    {
        private readonly IProductAttributeRepository _repository;
        private readonly ILogger _logger;

        public ProductAttributeService(IProductAttributeRepository repository, ILogger logger) : base(repository,
            logger)

        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<OperationResult<IEnumerable<ProductAttributeDto>>> GetFilterableAttributesAsync()
        {
            try
            {
                _logger.LogInformation("Getting filterable attributes");

                var allAttributes = await _repository.GetAll().ToListAsync();
                var filterableAttributes =
                    allAttributes.Where(attr => attr.IsFilterable).OrderBy(attr => attr.SortOrder);

                return OperationResult<IEnumerable<ProductAttributeDto>>.Success(filterableAttributes
                    .Adapt<IEnumerable<ProductAttributeDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filterable attributes");
                return OperationResult<IEnumerable<ProductAttributeDto>>.Fail("Error getting filterable attributes");
            }
        }

        public async Task<OperationResult<IEnumerable<ProductAttributeDto>>> GetRequiredAttributesAsync()
        {
            try
            {
                _logger.LogInformation("Getting required attributes");

                var requiredAttributes = await _repository.GetAll().Where(attr => attr.IsRequired)
                    .OrderBy(attr => attr.SortOrder).ToListAsync();

                return OperationResult<IEnumerable<ProductAttributeDto>>.Success(requiredAttributes
                    .Adapt<IEnumerable<ProductAttributeDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting required attributes");
                return OperationResult<IEnumerable<ProductAttributeDto>>.Fail("Error getting required attributes");
            }
        }
    }
}