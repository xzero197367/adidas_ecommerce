using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Main
{
    public class ProductAttributeService : GenericService<ProductAttribute, ProductAttributeDto, ProductAttributeCreateDto, ProductAttributeUpdateDto>, IProductAttributeService
    {
        private readonly IGenericRepository<ProductAttribute> _repository;

        public ProductAttributeService(IGenericRepository<ProductAttribute> repository, IMapper mapper, ILogger logger)
            : base(repository, mapper, logger)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductAttribute>> GetFilterableAttributesAsync()
        {
            try
            {
                _logger.LogInformation("Getting filterable attributes");

                var allAttributes = await _repository.GetAll().ToListAsync();
                var filterableAttributes = allAttributes.Where(attr => attr.IsFilterable).OrderBy(attr => attr.SortOrder);

                _logger.LogInformation("Found {Count} filterable attributes", filterableAttributes.Count());
                return filterableAttributes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filterable attributes");
                throw;
            }
        }

        public async Task<IEnumerable<ProductAttribute>> GetRequiredAttributesAsync()
        {
            try
            {
                _logger.LogInformation("Getting required attributes");

                var allAttributes = await _repository.GetAll().ToListAsync();
                var requiredAttributes = allAttributes.Where(attr => attr.IsRequired).OrderBy(attr => attr.SortOrder);

                _logger.LogInformation("Found {Count} required attributes", requiredAttributes.Count());
                return requiredAttributes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting required attributes");
                throw;
            }
        }

    }
}
