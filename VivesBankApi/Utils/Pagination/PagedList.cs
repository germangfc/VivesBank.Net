/// <summary>
/// Representa una lista de elementos paginados.
/// </summary>
/// <typeparam name="T">El tipo de los elementos en la lista paginada.</typeparam>
public class PagedList<T> : List<T>
{
    /// <summary>
    /// El número de página actual.
    /// </summary>
    public int PageNumber { get; private set; }

    /// <summary>
    /// El tamaño de cada página (número de elementos por página).
    /// </summary>
    public int PageSize { get; private set; }

    /// <summary>
    /// El número total de elementos en la colección original (antes de la paginación).
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// El número total de páginas basándose en el número total de elementos y el tamaño de la página.
    /// </summary>
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica si hay una página anterior.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 0;

    /// <summary>
    /// Indica si hay una página siguiente.
    /// </summary>
    public bool HasNextPage => PageNumber < PageCount - 1;

    /// <summary>
    /// Indica si la página actual es la primera página.
    /// </summary>
    public bool IsFirstPage => PageNumber == 0;

    /// <summary>
    /// Indica si la página actual es la última página.
    /// </summary>
    public bool IsLastPage => PageNumber == PageCount - 1;

    /// <summary>
    /// Constructor que crea una instancia de la lista paginada con los elementos proporcionados.
    /// </summary>
    /// <param name="items">La colección de elementos para agregar a la lista.</param>
    /// <param name="totalCount">El número total de elementos en la colección original.</param>
    /// <param name="pageNumber">El número de página actual.</param>
    /// <param name="pageSize">El tamaño de la página.</param>
    public PagedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        AddRange(items);  // Agregar los elementos a la lista
    }

    /// <summary>
    /// Método estático para crear una lista paginada a partir de una fuente de elementos.
    /// </summary>
    /// <param name="source">La colección de elementos a paginar.</param>
    /// <param name="pageNumber">El número de página a obtener.</param>
    /// <param name="pageSize">El tamaño de cada página.</param>
    /// <returns>Una nueva instancia de <see cref="PagedList{T}"/> con los elementos paginados.</returns>
    public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
