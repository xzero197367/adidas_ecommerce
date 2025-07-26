using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;
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
        Task<TDto?> GetByIdAsync(Guid id);
        Task<TDto?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IEnumerable<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TDto>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<PagedResultDto<TDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<PagedResultDto<TDto>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null);

        // Count operations
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        // Write operations
        Task<TDto> CreateAsync(TCreateDto createDto);
        Task<IEnumerable<TDto>> CreateRangeAsync(IEnumerable<TCreateDto> createDtos);
        Task<TDto> UpdateAsync(Guid id, TUpdateDto updateDto);
        Task<IEnumerable<TDto>> UpdateRangeAsync(IEnumerable<KeyValuePair<Guid, TUpdateDto>> updates);

        // Delete operations
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteAsync(TEntity entity);
        Task<int> DeleteRangeAsync(IEnumerable<Guid> ids);

        // Active/Inactive operations
        Task<bool> SetActiveStatusAsync(Guid id, bool isActive);
        Task<int> SetActiveStatusRangeAsync(IEnumerable<Guid> ids, bool isActive);
    }
}
