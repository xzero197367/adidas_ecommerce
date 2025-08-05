using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace Adidas.Application.Services.Main
{
    public class ProductAttributeValueService :
        GenericService<ProductAttributeValue, ProductAttributeValueDto, ProductAttributeValueCreateDto, ProductAttributeValueUpdateDto>,
        IProductAttributeValueService
    {
        private readonly IAttributeValueRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductAttributeValueService> _logger;

        public ProductAttributeValueService(
            IAttributeValueRepository repository,
            IMapper mapper,
            ILogger<ProductAttributeValueService> logger)
            : base(repository, mapper, logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductAttributeValueDto> CreateAsync(ProductAttributeValueCreateDto productAttributeValueCreateDto)
        {
            await ValidateCreateAsync(productAttributeValueCreateDto);

            var entity = _mapper.Map<ProductAttributeValue>(productAttributeValueCreateDto);
            await BeforeCreateAsync(entity);

            var created = await _repository.AddAsync(entity);
            return _mapper.Map<ProductAttributeValueDto>(created);
        }

        public async Task<IEnumerable<ProductAttributeValueDto>> CreateRangeAsync(IEnumerable<ProductAttributeValueCreateDto> createDtos)
        {
            var entities = _mapper.Map<IEnumerable<ProductAttributeValue>>(createDtos);
            await _repository.AddRangeAsync(entities);
            return _mapper.Map<IEnumerable<ProductAttributeValueDto>>(entities);
        }

        public async Task<ProductAttributeValue?> GetValueAsync(Guid valueId)
        {
            return await _repository.GetByIdAsync(valueId);
        }

        public async Task<IEnumerable<ProductAttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            return await _repository.GetValuesByAttributeIdAsync(attributeId);
        }

        public async Task<IEnumerable<ProductAttributeValue>> GetValuesByProductIdAsync(Guid productId)
        {
            return await _repository.GetValuesByProductIdAsync(productId);
        }

        public async Task<ProductAttributeValueDto> UpdateAsync(Guid id, ProductAttributeValueUpdateDto productAttributeValueUpdateDto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Product attribute value with ID {id} not found.");

            _mapper.Map(productAttributeValueUpdateDto, entity);
            await _repository.UpdateAsync(entity);

            return _mapper.Map<ProductAttributeValueDto>(entity);
        }

        public async Task<IEnumerable<ProductAttributeValueDto>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, ProductAttributeValueUpdateDto>> updates)
        {
            var result = new List<ProductAttributeValueDto>();

            foreach (var pair in updates)
            {
                var entity = await _repository.GetByIdAsync(pair.Key);
                if (entity == null) continue;

                _mapper.Map(pair.Value, entity);
                await _repository.UpdateAsync(entity);
                result.Add(_mapper.Map<ProductAttributeValueDto>(entity));
            }

            return result;
        }

        public override Task ValidateCreateAsync(ProductAttributeValueCreateDto productAttributeValueCreateDto)
        {
            if (string.IsNullOrWhiteSpace(productAttributeValueCreateDto.Value))
                throw new ArgumentException("Attribute value is required.");

            if (productAttributeValueCreateDto.ProductId == Guid.Empty)
                throw new ArgumentException("Product ID must be specified.");

            if (productAttributeValueCreateDto.AttributeId == Guid.Empty)
                throw new ArgumentException("Attribute ID must be specified.");

            return Task.CompletedTask;
        }

        public override Task BeforeCreateAsync(ProductAttributeValue entity)
        {
            // You can add logic like trimming value or checking uniqueness if needed
            entity.Value = entity.Value.Trim();
            return Task.CompletedTask;
        }
    }
}
