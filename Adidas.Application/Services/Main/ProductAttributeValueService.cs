using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adidas.Application.Contracts.RepositoriesContracts.Main;


namespace Adidas.Application.Services.Main
{
    public class ProductAttributeValueService :
        GenericService<ProductAttributeValue, ProductAttributeValueDto, CreateProductAttributeValueDto, UpdateProductAttributeValueDto>,
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

        public async Task<ProductAttributeValueDto> CreateAsync(CreateProductAttributeValueDto createDto)
        {
            await ValidateCreateAsync(createDto);

            var entity = _mapper.Map<ProductAttributeValue>(createDto);
            await BeforeCreateAsync(entity);

            var created = await _repository.AddAsync(entity);
            return _mapper.Map<ProductAttributeValueDto>(created);
        }

        public async Task<IEnumerable<ProductAttributeValueDto>> CreateRangeAsync(IEnumerable<CreateProductAttributeValueDto> createDtos)
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

        public async Task<ProductAttributeValueDto> UpdateAsync(Guid id, UpdateProductAttributeValueDto updateDto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Product attribute value with ID {id} not found.");

            _mapper.Map(updateDto, entity);
            await _repository.UpdateAsync(entity);

            return _mapper.Map<ProductAttributeValueDto>(entity);
        }

        public async Task<IEnumerable<ProductAttributeValueDto>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, UpdateProductAttributeValueDto>> updates)
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

        protected override Task ValidateCreateAsync(CreateProductAttributeValueDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Value))
                throw new ArgumentException("Attribute value is required.");

            if (createDto.ProductId == Guid.Empty)
                throw new ArgumentException("Product ID must be specified.");

            if (createDto.AttributeId == Guid.Empty)
                throw new ArgumentException("Attribute ID must be specified.");

            return Task.CompletedTask;
        }

        protected override Task BeforeCreateAsync(ProductAttributeValue entity)
        {
            // You can add logic like trimming value or checking uniqueness if needed
            entity.Value = entity.Value.Trim();
            return Task.CompletedTask;
        }
    }
}
