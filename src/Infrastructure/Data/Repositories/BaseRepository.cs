using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Domain.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> 
    where TEntity : BaseAuditableEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context switch
        {
            DbContext dbContext => dbContext.Set<TEntity>(),
            _ => throw new InvalidOperationException("Context must be a DbContext")
        };
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    

    public async Task<TEntity?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<TEntity>> AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _dbSet
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);

    public void DeleteRangeAsync(IReadOnlyList<TEntity> entitiesToRemove, CancellationToken cancellationToken = default) =>
        _dbSet.RemoveRange(entitiesToRemove);

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public IQueryable<TEntity> Queryable() =>
        _dbSet.AsQueryable();

    public IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters) =>
        _dbSet.FromSqlRaw(sql, parameters);
}
