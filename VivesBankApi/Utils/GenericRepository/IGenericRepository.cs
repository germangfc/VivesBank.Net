namespace ApiFunkosCS.Utils.GenericRepository;

/// <summary>
/// Interfaz genérica para operaciones básicas de repositorio con soporte para entidades de tipo <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Tipo de la entidad que se va a almacenar en el repositorio.</typeparam>
/// <remarks>
/// Esta interfaz define métodos comunes para realizar operaciones CRUD sobre entidades de tipo <typeparamref name="T"/>.
/// </remarks>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Obtiene una entidad por su identificador.
    /// </summary>
    /// <param name="id">El identificador de la entidad.</param>
    /// <returns>La entidad de tipo <typeparamref name="T"/> si se encuentra, de lo contrario, <c>null</c>.</returns>
    Task<T?> GetByIdAsync(String id);

    /// <summary>
    /// Obtiene todas las entidades de tipo <typeparamref name="T"/> del repositorio.
    /// </summary>
    /// <returns>Una lista de todas las entidades de tipo <typeparamref name="T"/>.</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Obtiene las entidades de tipo <typeparamref name="T"/> paginadas.
    /// </summary>
    /// <param name="pageNumber">El número de la página para la paginación.</param>
    /// <param name="pageSize">El tamaño de la página.</param>
    /// <returns>Un objeto <see cref="PagedList{T}"/> que contiene las entidades de la página solicitada.</returns>
    Task<PagedList<T>> GetAllPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Agrega una nueva entidad al repositorio.
    /// </summary>
    /// <param name="entity">La entidad a agregar.</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Actualiza una entidad existente en el repositorio.
    /// </summary>
    /// <param name="entity">La entidad a actualizar.</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Elimina una entidad del repositorio por su identificador.
    /// </summary>
    /// <param name="id">El identificador de la entidad a eliminar.</param>
    Task DeleteAsync(String id);

    /// <summary>
    /// Elimina todas las entidades del repositorio de manera segura.
    /// </summary>
    Task DeleteAllAsync();
}
