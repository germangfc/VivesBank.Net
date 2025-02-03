/// <summary>
/// Representa una respuesta paginada para una lista de elementos de tipo <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">El tipo de los elementos en la página.</typeparam>
public class PageResponse<T>
{
    /// <summary>
    /// La lista de elementos contenidos en la página.
    /// </summary>
    public List<T> Content { get; set; }

    /// <summary>
    /// El número total de páginas disponibles en la consulta.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// El número total de elementos en la colección completa.
    /// </summary>
    public long TotalElements { get; set; }

    /// <summary>
    /// El tamaño de la página (número de elementos por página).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// El número de la página actual.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// El número total de elementos que contiene la página actual.
    /// </summary>
    public int TotalPageElements { get; set; }

    /// <summary>
    /// Indica si la página está vacía.
    /// </summary>
    public bool Empty { get; set; }

    /// <summary>
    /// Indica si la página actual es la primera.
    /// </summary>
    public bool First { get; set; }

    /// <summary>
    /// Indica si la página actual es la última.
    /// </summary>
    public bool Last { get; set; }

    /// <summary>
    /// El nombre del campo utilizado para ordenar los elementos en la página.
    /// </summary>
    public string SortBy { get; set; }

    /// <summary>
    /// La dirección de ordenamiento de la página (ascendente o descendente).
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// Crea una instancia de <see cref="PageResponse{T}"/> a partir de un objeto de tipo <see cref="PagedList{T}"/>.
    /// </summary>
    /// <param name="page">La página de elementos que contiene la lista de elementos.</param>
    /// <param name="sortBy">El nombre del campo por el cual se ordenan los elementos.</param>
    /// <param name="direction">La dirección de ordenamiento de los elementos (ascendente o descendente).</param>
    /// <returns>Una instancia de <see cref="PageResponse{T}"/> con la información paginada.</returns>
    public static PageResponse<T> FromPage<T>(PagedList<T> page, string sortBy, string direction)
    {
        return new PageResponse<T>
        {
            Content = page,
            TotalPages = page.PageCount,
            TotalElements = page.TotalCount,
            PageSize = page.PageSize,
            PageNumber = page.PageNumber,
            TotalPageElements = page.Count,
            Empty = page.Count == 0,
            First = page.IsFirstPage,
            Last = page.IsLastPage,
            SortBy = sortBy,
            Direction = direction
        };
    }
}
