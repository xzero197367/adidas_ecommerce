


using Adidas.Models;
using System.Linq.Expressions;

namespace Adidas.Infra
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // Read operations
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        // Pagination
        Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);

        // Count operations
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Write operations
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task<T> UpdateAsync(T entity);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);

        // Soft delete operations
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(T entity);
        Task<int> SoftDeleteRangeAsync(IEnumerable<T> entities);

        // Hard delete operations (use with caution)
        Task<bool> HardDeleteAsync(Guid id);
        Task<bool> HardDeleteAsync(T entity);
        Task<int> HardDeleteRangeAsync(IEnumerable<T> entities);

        // Active/Inactive operations
        Task<bool> SetActiveStatusAsync(Guid id, bool isActive);
        Task<int> SetActiveStatusRangeAsync(IEnumerable<Guid> ids, bool isActive);

        // Query building
        IQueryable<T> GetQueryable();
        IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate);
    }
}
