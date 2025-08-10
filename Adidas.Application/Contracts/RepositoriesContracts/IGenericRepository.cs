using Adidas.Models;
using System.Linq.Expressions;
using Adidas.DTOs.Common_DTOs;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Adidas.Application.Contracts.RepositoriesContracts
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // Read operations
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetAll(Func<IQueryable<T>, IQueryable<T>>? queryFunc = null);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);


        Task<T?> FindAsync(Func<IQueryable<T>, IQueryable<T>> queryFunc);

        // Pagination
        Task<PagedResultDto<T>> GetPagedAsync(int pageNumber, int pageSize, Func<IQueryable<T>, IQueryable<T>>? queryFunc = null);
        
        // Count operations
        // Task<int> CountAsync(); // todo: optional where
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Write operations
        Task<EntityEntry<T>> AddAsync(T entity);
        Task<IEnumerable<EntityEntry<T>>> AddRangeAsync(IEnumerable<T> entities);
        Task<EntityEntry<T>> UpdateAsync(T entity);
        Task<IEnumerable<EntityEntry<T>>> UpdateRangeAsync(IEnumerable<T> entities);

        // Soft delete operations

        Task<EntityEntry<T>> SoftDeleteAsync(Guid id);

        // Task<bool> SoftDeleteAsync(T entity);                    
        IEnumerable<EntityEntry<T>> SoftDeleteRange(IEnumerable<T> entities);

        // Hard delete operations (use with caution)
        Task<EntityEntry<T>> HardDeleteAsync(Guid id);

        // Task<bool> HardDeleteAsync(T entity);
        Task<IEnumerable<EntityEntry<T>>> HardDeleteRangeAsync(IEnumerable<T> entities);

        // Save changes
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}