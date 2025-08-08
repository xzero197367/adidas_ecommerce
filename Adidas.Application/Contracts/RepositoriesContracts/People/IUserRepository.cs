
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models.People;
using System.Linq.Expressions;

namespace Adidas.Application.Contracts.RepositoriesContracts.People;


public interface IUserRepository
{
    
    // Read operations
    Task<User?> GetByIdAsync(string id, params Expression<Func<User, object>>[] includes);
    IQueryable<User> GetAll(Func<IQueryable<User>, IQueryable<User>>? queryFunc = null);
    Task<User?> FindAsync(Func<IQueryable<User>, IQueryable<User>> queryFunc);

    // Pagination
    Task<(IEnumerable<User> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Func<IQueryable<User>, IQueryable<User>>? queryFunc = null);
        
    // Count operations
    // Task<int> CountAsync(); // todo: optional where
    Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);

    // Write operations
    Task<EntityEntry<User>> AddAsync(User entity);
    Task<IEnumerable<EntityEntry<User>>> AddRangeAsync(IEnumerable<User> entities);
    Task<EntityEntry<User>> UpdateAsync(User entity);
    Task<IEnumerable<EntityEntry<User>>> UpdateRangeAsync(IEnumerable<User> entities);

    // Soft delete operations

    Task<EntityEntry<User>> SoftDeleteAsync(string id);

    // Task<bool> SoftDeleteAsync(T entity);                    
    IEnumerable<EntityEntry<User>> SoftDeleteRange(IEnumerable<User> entities);

    // Hard delete operations (use with caution)
    Task<EntityEntry<User>> HardDeleteAsync(string id);

    // Task<bool> HardDeleteAsync(T entity);
    Task<IEnumerable<EntityEntry<User>>> HardDeleteRangeAsync(IEnumerable<User> entities);

    // Save changes
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}