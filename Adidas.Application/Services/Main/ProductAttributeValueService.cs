using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.Models.Main;
using Mapster;
using Microsoft.Extensions.Logging;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;


namespace Adidas.Application.Services.Main;

public class ProductAttributeValueService :
    GenericService<ProductAttributeValue, ProductAttributeValueDto, ProductAttributeValueCreateDto,
        ProductAttributeValueUpdateDto>, IProductAttributeValueService
{
    private readonly IProductAttributeValueRepository _repository;
    private readonly ILogger<ProductAttributeValueService> _logger;

    public ProductAttributeValueService(
        IProductAttributeValueRepository repository,
        ILogger<ProductAttributeValueService> logger) : base(repository, logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OperationResult<ProductAttributeValueDto>> CreateAsync(
        ProductAttributeValueCreateDto productAttributeValueCreateDto)
    {
        try
        {
            await ValidateCreateAsync(productAttributeValueCreateDto);

            var entity = productAttributeValueCreateDto.Adapt<ProductAttributeValue>();
            await BeforeCreateAsync(entity);

            var created = await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            created.State = EntityState.Detached;
            return OperationResult<ProductAttributeValueDto>.Success(
                created.Entity.Adapt<ProductAttributeValueDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product attribute value");
            return OperationResult<ProductAttributeValueDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> CreateRangeAsync(
        IEnumerable<ProductAttributeValueCreateDto> createDtos)
    {
        try
        {
            var entities = createDtos.Adapt<IEnumerable<ProductAttributeValue>>();
            var created = await _repository.AddRangeAsync(entities);
            await _repository.SaveChangesAsync();
            var createdEntities = created.Select(x =>
            {
                x.State = EntityState.Detached;
                return x.Entity;
            });
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Success(
                createdEntities.Adapt<IEnumerable<ProductAttributeValueDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product attribute values");
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<ProductAttributeValueDto>> GetValueAsync(Guid valueId)
    {
        try
        {
            var value = await _repository.GetByIdAsync(valueId);
            return OperationResult<ProductAttributeValueDto>.Success(value.Adapt<ProductAttributeValueDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product attribute value");
            return OperationResult<ProductAttributeValueDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> GetValuesByAttributeIdAsync(
        Guid attributeId)
    {
        try
        {
            var result = await _repository.GetValuesByAttributeIdAsync(attributeId);
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Success(
                result.Adapt<IEnumerable<ProductAttributeValueDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product attribute values");
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> GetValuesByProductIdAsync(
        Guid productId)
    {
        try
        {
            var result = await _repository.GetValuesByProductIdAsync(productId);
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Success(
                result.Adapt<IEnumerable<ProductAttributeValueDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product attribute values");
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<ProductAttributeValueDto>> UpdateAsync(Guid id,
        ProductAttributeValueUpdateDto productAttributeValueUpdateDto)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Product attribute value with ID {id} not found.");

            var updateElement = productAttributeValueUpdateDto.Adapt<ProductAttributeValue>();
            var result = await _repository.UpdateAsync(updateElement);
            await _repository.SaveChangesAsync();
            result.State = EntityState.Detached;
            return OperationResult<ProductAttributeValueDto>.Success(
                result.Entity.Adapt<ProductAttributeValueDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product attribute value");
            return OperationResult<ProductAttributeValueDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<IEnumerable<ProductAttributeValueDto>>> UpdateRangeAsync(
        IEnumerable<ProductAttributeValueUpdateDto> updates)
    {
        try
        {
            var elementsToUpdate = updates.Adapt<IEnumerable<ProductAttributeValue>>();
            var result = await _repository.UpdateRangeAsync(elementsToUpdate);
            var updatedEntities = result.Select(x =>
            {
                x.State = EntityState.Detached;
                return x.Entity;
            });

            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Success(
                updatedEntities.Adapt<IEnumerable<ProductAttributeValueDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product attribute values");
            return OperationResult<IEnumerable<ProductAttributeValueDto>>.Fail(ex.Message);
        }
    }

    public Task ValidateCreateAsync(ProductAttributeValueCreateDto productAttributeValueCreateDto)
    {
        if (string.IsNullOrWhiteSpace(productAttributeValueCreateDto.Value))
            throw new ArgumentException("Attribute value is required.");

        if (productAttributeValueCreateDto.ProductId == Guid.Empty)
            throw new ArgumentException("Product ID must be specified.");

        if (productAttributeValueCreateDto.AttributeId == Guid.Empty)
            throw new ArgumentException("Attribute ID must be specified.");

        return Task.CompletedTask;
    }

    public Task BeforeCreateAsync(ProductAttributeValue entity)
    {
        // You can add logic like trimming value or checking uniqueness if needed
        entity.Value = entity.Value.Trim();
        return Task.CompletedTask;
    }
}