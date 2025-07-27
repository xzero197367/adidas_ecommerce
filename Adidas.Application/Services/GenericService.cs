
using System.Linq.Expressions;
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.ServicesContracts;
using Adidas.DTOs.Common_DTOs;
using Adidas.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services
{
    public abstract class GenericService<TEntity, TDto, TCreateDto, TUpdateDto> : IGenericService<TEntity, TDto, TCreateDto, TUpdateDto>
         where TEntity : BaseEntity
         where TDto : class
         where TCreateDto : class
         where TUpdateDto : class
    {
        protected readonly IGenericRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected GenericService(IGenericRepository<TEntity> repository, IMapper mapper, ILogger logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task<TDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by id {Id}", id);
                throw;
            }
        }

        public virtual async Task<TDto?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id, includes);
                return entity == null ? null : _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by id {Id} with includes", id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<TDto>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities");
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                var entities = await _repository.GetAllAsync(includes);
                return _mapper.Map<IEnumerable<TDto>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities with includes");
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                var entities = await _repository.FindAsync(predicate);
                return _mapper.Map<IEnumerable<TDto>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities with predicate");
                throw;
            }
        }

        public virtual async Task<PagedResultDto<TDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize);
                return new PagedResultDto<TDto>
                {
                    Items = _mapper.Map<IEnumerable<TDto>>(items),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities");
                throw;
            }
        }

        public virtual async Task<PagedResultDto<TDto>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null)
        {
            try
            {
                var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize, predicate);
                return new PagedResultDto<TDto>
                {
                    Items = _mapper.Map<IEnumerable<TDto>>(items),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities with predicate");
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _repository.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities");
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return await _repository.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities with predicate");
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return await _repository.ExistsAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking entity existence");
                throw;
            }
        }

        public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
        {
            try
            {
                await ValidateCreateAsync(createDto);

                var entity = _mapper.Map<TEntity>(createDto);
                await BeforeCreateAsync(entity);

                var createdEntityEntry = await _repository.AddAsync(entity);
                var createdEntity = createdEntityEntry.Entity; // Extract the entity from EntityEntry
                await AfterCreateAsync(createdEntity);

                return _mapper.Map<TDto>(createdEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity");
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> CreateRangeAsync(IEnumerable<TCreateDto> createDtos)
        {
            try
            {
                var createDtoList = createDtos.ToList();
                foreach (var createDto in createDtoList)
                {
                    await ValidateCreateAsync(createDto);
                }

                var entities = _mapper.Map<IEnumerable<TEntity>>(createDtoList);
                var entityList = entities.ToList();

                foreach (var entity in entityList)
                {
                    await BeforeCreateAsync(entity);
                }

                var createdEntityEntries = await _repository.AddRangeAsync(entityList);
                var createdEntityList = createdEntityEntries.Select(entry => entry.Entity).ToList(); // Extract entities from EntityEntry collection

                foreach (var createdEntity in createdEntityList)
                {
                    await AfterCreateAsync(createdEntity);
                }

                return _mapper.Map<IEnumerable<TDto>>(createdEntityList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entities");
                throw;
            }
        }

        public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto updateDto)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    throw new KeyNotFoundException($"Entity with id {id} not found");

                await ValidateUpdateAsync(id, updateDto);

                _mapper.Map(updateDto, existingEntity);
                await BeforeUpdateAsync(existingEntity);

                var updatedEntityEntry = await _repository.UpdateAsync(existingEntity);
                var updatedEntity = updatedEntityEntry.Entity; // Extract the entity from EntityEntry
                await AfterUpdateAsync(updatedEntity);

                return _mapper.Map<TDto>(updatedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity with id {Id}", id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, TUpdateDto>> updates)
        {
            try
            {
                var updateList = updates.ToList();
                var entities = new List<TEntity>();

                foreach (var update in updateList)
                {
                    var existingEntity = await _repository.GetByIdAsync(update.Key);
                    if (existingEntity == null)
                        throw new KeyNotFoundException($"Entity with id {update.Key} not found");

                    await ValidateUpdateAsync(update.Key, update.Value);
                    _mapper.Map(update.Value, existingEntity);
                    await BeforeUpdateAsync(existingEntity);
                    entities.Add(existingEntity);
                }

                var updatedEntityEntries = await _repository.UpdateRangeAsync(entities);
                var updatedEntityList = updatedEntityEntries.Select(entry => entry.Entity).ToList(); // Extract entities from EntityEntry collection

                foreach (var updatedEntity in updatedEntityList)
                {
                    await AfterUpdateAsync(updatedEntity);
                }

                return _mapper.Map<IEnumerable<TDto>>(updatedEntityList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entities");
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null) return false;

                await BeforeDeleteAsync(entity);
                var result = await _repository.SoftDeleteAsync(id);
                if (result)
                {
                    await AfterDeleteAsync(entity);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity with id {Id}", id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(TEntity entity)
        {
            try
            {
                await BeforeDeleteAsync(entity);
                var result = await _repository.SoftDeleteAsync(entity.Id);
                if (result)
                {
                    await AfterDeleteAsync(entity);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity");
                throw;
            }
        }

        public virtual async Task<int> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            try
            {
                var entities = new List<TEntity>();
                foreach (var id in ids)
                {
                    var entity = await _repository.GetByIdAsync(id);
                    if (entity != null)
                    {
                        await BeforeDeleteAsync(entity);
                        entities.Add(entity);
                    }
                }

                var result = await _repository.SoftDeleteRangeAsync(entities);

                foreach (var entity in entities)
                {
                    await AfterDeleteAsync(entity);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entities");
                throw;
            }
        }

        public virtual async Task<bool> SetActiveStatusAsync(Guid id, bool isActive)
        {
            try
            {
                return await _repository.SetActiveStatusAsync(id, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active status for entity {Id}", id);
                throw;
            }
        }

        public virtual async Task<int> SetActiveStatusRangeAsync(IEnumerable<Guid> ids, bool isActive)
        {
            try
            {
                return await _repository.SetActiveStatusRangeAsync(ids, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active status for entities");
                throw;
            }
        }

        // Virtual methods for customization in derived classes
        protected virtual Task ValidateCreateAsync(TCreateDto createDto) => Task.CompletedTask;
        protected virtual Task ValidateUpdateAsync(Guid id, TUpdateDto updateDto) => Task.CompletedTask;
        protected virtual Task BeforeCreateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task AfterCreateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task BeforeUpdateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task AfterUpdateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task BeforeDeleteAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task AfterDeleteAsync(TEntity entity) => Task.CompletedTask;
    }
}
