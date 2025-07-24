using Adidas.Context;
using Adidas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using Adidas.Application.Contracts.RepositoriesContracts;

namespace Adidas.Infra;

public class GenericRepository<T> : IGenericRepository<T>  where T : BaseEntity
{
    protected readonly AdidasDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AdidasDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region  Write operations

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

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with id {id} not found");
        }

        entity.IsDeleted = true;
        var updatedEntity = await UpdateAsync(entity);

        if (updatedEntity?.Entity?.IsDeleted == true)
        {
            return true;
        }

        return false;
    }

    public async Task<int> SoftDeleteRangeAsync(IEnumerable<T> entities)
    {
        List<bool> updatedEntities = new();
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            updatedEntities.Add(await SoftDeleteAsync(entity.Id));
        }
        return updatedEntities.Count;
    }

    #endregion

    #region Hard delete operations (use with caution)

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with id {id} not found");
        }
        _dbSet.Remove(entity);
        return true;
    }
    
    public async Task<int> HardDeleteRangeAsync(IEnumerable<T> entities)
    {
        List<bool> deletedEntities = new();
        foreach (var entity in entities)
        {
            deletedEntities.Add(await HardDeleteAsync(entity.Id));
        }
        return deletedEntities.Count;
    }

    #endregion

    #region Query building

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    } 
    
    public IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsQueryable().Where(predicate);
    }

    #endregion

    #region Count operations

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
    
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    #endregion

    #region Pagination

    public async Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var items = await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalCount = await _dbSet.CountAsync();
        return (items, totalCount);
    }
    
    public async Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
    {
        var items = await _dbSet.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalCount = await _dbSet.Where(predicate).CountAsync();
        return (items, totalCount);
    }

    #endregion

    #region Read operations

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(x => x.Id == id);
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

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }
    
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }
    
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    #endregion
    
}