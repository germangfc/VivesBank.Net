using Microsoft.EntityFrameworkCore;

namespace ApiFunkosCS.Utils.GenericRepository;

public class GenericRepository<C, T> : IGenericRepository<T> where T : class where C : DbContext
{
    private readonly C _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger _logger;

    public GenericRepository(C context, ILogger logger)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _logger = logger;
    }

    public async Task<List<T>> GetAllAsync()
    {
        _logger.LogInformation($"Getting all {typeof(T).Name}s");
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(String id)
    {
        _logger.LogInformation($"Getting {typeof(T).Name} with id {id}");
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        _logger.LogInformation($"Adding {typeof(T).Name}");
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _logger.LogInformation($"Updating {typeof(T).Name} entity: {entity}");
        _context.Entry(entity).State = EntityState.Modified; // Cambiamos el estado explícitamente
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(String id)
    {
        _logger.LogInformation($"Deleting {typeof(T).Name} with id {id}");
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            _logger.LogWarning($"No {typeof(T).Name} found with id {id}");
            return;
        }
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
    
    private static readonly SemaphoreSlim _semaphore = new(1, 1); // (capacidad inicial, capacidad máxima)

    public async Task DeleteAllAsync()
    {
        await _semaphore.WaitAsync(); // 🔒 Bloquea la sección crítica de forma asíncrona
        try
        {
            await _dbSet.ExecuteDeleteAsync();
        }
        finally
        {
            _semaphore.Release(); // 🔓 Libera el acceso
        }
    }



}
