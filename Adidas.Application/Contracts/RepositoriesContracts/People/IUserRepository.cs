
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models.People;
using System.Linq.Expressions;

namespace Adidas.Application.Contracts.RepositoriesContracts.People;


public interface IUserRepository
{
    Task< User?> GetByIdAsync(string id);
    Task< User?> GetByIdAsync(string id, params Expression<Func<User, object>>[] includes);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllAsync(params Expression<Func<User, object>>[] includes);
    Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate);
    Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includes);
    Task< User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate);
    Task< User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includes);

    // Pagination
    Task<(IEnumerable<User> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<User> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<User, bool>>? predicate = null);

    // Count operations
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<User, bool>> predicate);
    // Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);        /// بتعات ايه دي

    // Write operations
    Task<EntityEntry<User>> AddAsync(User entity);
    Task<IEnumerable<EntityEntry<User>>> AddRangeAsync(IEnumerable<User> entities);
    Task<EntityEntry<User>> UpdateAsync(User entity);
    Task<IEnumerable<EntityEntry<User>>> UpdateRangeAsync(IEnumerable<User> entities);

    // Soft delete operations
    Task<bool> SoftDeleteAsync(Guid id);
    // Task<bool> SoftDeleteAsync(User entity);                    
    Task<int> SoftDeleteRangeAsync(IEnumerable<User> entities);

    // Hard delete operations (use with caution)
    Task<bool> HardDeleteAsync(Guid id);
    // Task<bool> HardDeleteAsync(User entity);
    Task<int> HardDeleteRangeAsync(IEnumerable<User> entities);

    // Active/Inactive operations
    // Task<bool> SetActiveStatusAsync(Guid id, bool isActive);         /// بتعات ايه دي
    // Task<int> SetActiveStatusRangeAsync(IEnumerable<Guid> ids, bool isActive);   /// بتعات ايه دي

    // Query building
    IQueryable<User> GetQueryable();
    IQueryable<User> GetQueryable(Expression<Func<User, bool>> predicate);

}