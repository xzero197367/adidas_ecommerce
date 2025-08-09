using System.Linq.Expressions;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models.People;

namespace Adidas.Infra.People;

public class UserRepository: IUserRepository
{
     protected readonly AdidasDbContext _context;
    protected readonly DbSet<User> _dbSet;

    public UserRepository(AdidasDbContext context)
    {
        _context = context;
        _dbSet = context.Set<User>();
    }

    #region Write operations

    public Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
    {
        return _context.Set<User>().AnyAsync(predicate);
    }

    public async Task<EntityEntry<User>> AddAsync(User entity)
    {
        var createdEntity = await _context.AddAsync(entity);

        return createdEntity;
    }

    public async Task<IEnumerable<EntityEntry<User>>> AddRangeAsync(IEnumerable<User> entities)
    {
        List<EntityEntry<User>> createdEntities = new();
        foreach (var entity in entities)
        {
            createdEntities.Add(await _context.AddAsync(entity));
        }

        return createdEntities;
    }

    public async Task<EntityEntry<User>> UpdateAsync(User entity)
    {
        var updatedEntity = _context.Update(entity);

        return updatedEntity;
    }

    public async Task<IEnumerable<EntityEntry<User>>> UpdateRangeAsync(IEnumerable<User> entities)
    {
        List<EntityEntry<User>> updatedEntities = new();
        foreach (var entity in entities)
        {
            updatedEntities.Add(_context.Update(entity));
        }

        return updatedEntities;
    }

    #endregion

    #region Soft delete operations

    public async Task<EntityEntry<User>> SoftDeleteAsync(string id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with id {id} not found");
        }

        entity.IsDeleted = true;
        var updatedEntity = _context.Update(entity);

        return updatedEntity;
    }

    public IEnumerable<EntityEntry<User>> SoftDeleteRange(IEnumerable<User> entities)
    {
        List<EntityEntry<User>> updatedEntities = new();
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            updatedEntities.Add(_context.Update(entity));
        }

        return updatedEntities;
    }

    #endregion

    #region Hard delete operations (use with caution)

    public async Task<EntityEntry<User>> HardDeleteAsync(string id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with id {id} not found");
        }
        return _dbSet.Remove(entity);
    }

    public async Task<IEnumerable<EntityEntry<User>>> HardDeleteRangeAsync(IEnumerable<User> entities)
    {
        List<EntityEntry<User>> deletedEntities = new();
        foreach (var entity in entities)
        {
            deletedEntities.Add(_dbSet.Remove(entity));
        }
        return deletedEntities;
    }

    #endregion

    #region Save changes

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception here if needed
            throw new Exception(
                "An error occurred while saving changes to the database. See the inner exception for details.", ex);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Operation was explicitly cancelled
            throw new OperationCanceledException("The operation was cancelled by the user.", cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception here if needed
            throw new Exception(
                "An error occurred while saving changes to the database. See the inner exception for details.", ex);
        }
    }

    #endregion

    #region Count operations

    public async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null)
    {
        var query = _dbSet.AsQueryable();
        if (predicate != null) query = query.Where(predicate);
        return await query.CountAsync();
    }

    #endregion

    #region Pagination

    public async Task<(IEnumerable<User> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize,
        Func<IQueryable<User>, IQueryable<User>>? queryFunc = null)
    {
        var query = _dbSet.AsQueryable();
        if (queryFunc != null) query = queryFunc(query);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    #endregion

    #region Read operations

    public async Task<User?> GetByIdAsync(string id, params Expression<Func<User, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<User> GetAll(Func<IQueryable<User>, IQueryable<User>>? queryFunc = null)
    {
        var query = _dbSet.AsQueryable();
        if (queryFunc != null) query = queryFunc(query);
        return query;
    }

    public async Task<User?> FindAsync(Func<IQueryable<User>, IQueryable<User>> queryFunc)
    {
        var query = _dbSet.AsQueryable();
        query = queryFunc(query);
        return await query.FirstOrDefaultAsync();
    }
    
    #endregion
}