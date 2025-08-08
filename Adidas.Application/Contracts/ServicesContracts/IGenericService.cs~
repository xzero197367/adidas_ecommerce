
using System.Linq.Expressions;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.Models;

namespace Adidas.Application.Contracts.ServicesContracts
{
    public interface IGenericService<TEntity, TDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        // Read operations

        Task<OperationResult<TDto>> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);

        Task<OperationResult<IEnumerable<TDto>>>
            GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryFunc = null);

        Task<OperationResult<IEnumerable<TDto>>> FindAsync(Func<IQueryable<TEntity>,IQueryable<TEntity>> queryFunc);
       

        Task<OperationResult<PagedResultDto<TDto>>>
            GetPagedAsync(int pageNumber, int pageSize,
                Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryFunc = null);

        // Count operations
        Task<OperationResult<int>> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
        Task<OperationResult<bool>> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        // Write operations
        Task<OperationResult<TDto>> CreateAsync(TCreateDto createDto);
        Task<OperationResult<IEnumerable<TDto>>> CreateRangeAsync(IEnumerable<TCreateDto> createDtos);
        Task<OperationResult<TDto>> UpdateAsync(Guid id, TUpdateDto updateDto);
        Task<OperationResult<IEnumerable<TDto>>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, TUpdateDto>> updates);

        // Delete operations
        Task<OperationResult<TEntity>> DeleteAsync(Guid id);
        Task<OperationResult<TEntity>> DeleteAsync(TEntity entity);
        Task<OperationResult<IEnumerable<TEntity>>> DeleteRangeAsync(IEnumerable<Guid> ids);
        
    }
}