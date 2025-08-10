using Adidas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.DTOs.Common_DTOs;

namespace Adidas.Infra;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseAuditableEntity
{
    protected readonly AdidasDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AdidasDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Write operations

    public async Task<EntityEntry<T>> AddAsync(T entity)
    {
        var createdEntity = await _context.AddAsync(entity);

        return createdEntity;
    }

    public async Task<IEnumerable<EntityEntry<T>>> AddRangeAsync(IEnumerable<T> entities)
    {
        List<EntityEntry<T>> createdEntities = new();
        foreach (var entity in entities)
        {
            createdEntities.Add(await _context.AddAsync(entity));
        }

        return createdEntities;
    }

    public async Task<EntityEntry<T>> UpdateAsync(T entity)
    {
        var updatedEntity = _context.Update(entity);

        return updatedEntity;
    }

    public async Task<IEnumerable<EntityEntry<T>>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        List<EntityEntry<T>> updatedEntities = new();
        foreach (var entity in entities)
        {
            updatedEntities.Add(_context.Update(entity));
        }

        return updatedEntities;
    }

    #endregion

    #region Soft delete operations

    public async Task<EntityEntry<T>> SoftDeleteAsync(Guid id)
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

    public IEnumerable<EntityEntry<T>> SoftDeleteRange(IEnumerable<T> entities)
    {
        List<EntityEntry<T>> updatedEntities = new();
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            updatedEntities.Add(_context.Update(entity));
        }

        return updatedEntities;
    }

    #endregion

    #region Hard delete operations (use with caution)

    public async Task<EntityEntry<T>> HardDeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with id {id} not found");
        }

        return _dbSet.Remove(entity);
    }

    public async Task<IEnumerable<EntityEntry<T>>> HardDeleteRangeAsync(IEnumerable<T> entities)
    {
        List<EntityEntry<T>> deletedEntities = new();
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

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.AsQueryable();
        if (predicate != null) query = query.Where(predicate);
        return await query.CountAsync();
    }

    #endregion

    #region Pagination

    public async Task<PagedResultDto<T>> GetPagedAsync(int pageNumber, int pageSize,
        Func<IQueryable<T>, IQueryable<T>>? queryFunc = null)
    {
        var query = _dbSet.AsQueryable();
        if (queryFunc != null) query = queryFunc(query);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    #endregion

    #region Read operations

    public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public  IQueryable<T> GetAll(Func<IQueryable<T>, IQueryable<T>>? queryFunc = null)
    {
        var query = _dbSet.AsQueryable();
        if (queryFunc != null) query = queryFunc(query);
        return query;
    }

    public async Task<T?> FindAsync(Func<IQueryable<T>, IQueryable<T>> queryFunc)
    {
        var query = _dbSet.AsQueryable();
        query = queryFunc(query);
        return await query.FirstOrDefaultAsync();
    }

    Task<bool> IGenericRepository<T>.ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync();
    }


    #endregion
}