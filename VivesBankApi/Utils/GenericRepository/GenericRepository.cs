using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;


/// <summary>
/// Implementación genérica de un repositorio para manejar operaciones CRUD.
/// </summary>
/// <typeparam name="C">Tipo de contexto de base de datos (heredado de <see cref="DbContext"/>).</typeparam>
/// <typeparam name="T">Tipo de la entidad que se va a almacenar en la base de datos.</typeparam>
/// <remarks>
/// Esta clase proporciona métodos comunes para obtener, agregar, actualizar y eliminar entidades
/// de la base de datos, así como soporte para paginación y bloqueo de operaciones simultáneas en la base de datos.
/// </remarks>
public class GenericRepository<C, T> : IGenericRepository<T> where T : class where C : DbContext
{
    protected readonly C _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio genérico.
    /// </summary>
    /// <param name="context">El contexto de la base de datos.</param>
    /// <param name="logger">El servicio de registro utilizado para registrar las operaciones.</param>
    public GenericRepository(C context, ILogger logger)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las entidades de la base de datos.
    /// </summary>
    /// <returns>Una lista de todas las entidades de tipo <typeparamref name="T"/>.</returns>
    public async Task<List<T>> GetAllAsync()
    {
        _logger.LogInformation($"Getting all {typeof(T).Name}s");
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Obtiene las entidades paginadas de la base de datos.
    /// </summary>
    /// <param name="pageNumber">El número de página para la paginación.</param>
    /// <param name="pageSize">El tamaño de la página.</param>
    /// <returns>Una lista de entidades paginadas de tipo <typeparamref name="T"/>.</returns>
    public async Task<PagedList<T>> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(pageNumber * pageSize)  // Desplazamiento
            .Take(pageSize)              // Número de elementos por página
            .ToListAsync();

        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Obtiene una entidad por su identificador.
    /// </summary>
    /// <param name="id">El identificador de la entidad.</param>
    /// <returns>La entidad de tipo <typeparamref name="T"/> si se encuentra, de lo contrario, <c>null</c>.</returns>
    public async Task<T?> GetByIdAsync(String id)
    {
        _logger.LogInformation($"Getting {typeof(T).Name} with id {id}");
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Agrega una nueva entidad a la base de datos.
    /// </summary>
    /// <param name="entity">La entidad a agregar.</param>
    public async Task AddAsync(T entity)
    {
        _logger.LogInformation($"Adding {typeof(T).Name}");
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza una entidad existente en la base de datos.
    /// </summary>
    /// <param name="entity">La entidad a actualizar.</param>
    public async Task UpdateAsync(T entity)
    {
        _logger.LogInformation($"Updating {typeof(T).Name} entity: {entity}");
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una entidad de la base de datos por su identificador.
    /// </summary>
    /// <param name="id">El identificador de la entidad a eliminar.</param>
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

    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Elimina todas las entidades de la base de datos de manera segura, utilizando bloqueo para evitar operaciones simultáneas.
    /// </summary>
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

