using JokerNutrition.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Data.Repositories;

public interface _IBaseRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryAll();
    Task<TEntity?> GetByIdAsync(int id);
    Task CreateAsync(TEntity entity);
    Task CreateRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task SaveChangesAsync();
}

public abstract class _BaseRepository<TEntity> : _IBaseRepository<TEntity>
    where TEntity : class
{
    protected readonly JokerNutritionContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;

    protected _BaseRepository(JokerNutritionContext context, ILogger logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger;
    }

    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();
    public IQueryable<TEntity> QueryAll() => _dbSet.AsQueryable().AsNoTracking();

    public async Task<TEntity?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task CreateAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task CreateRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
