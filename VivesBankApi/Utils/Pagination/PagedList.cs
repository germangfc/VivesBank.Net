public class PagedList<T> : List<T>
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 0;
    public bool HasNextPage => PageNumber < PageCount - 1;

    public bool IsFirstPage => PageNumber == 0;
    public bool IsLastPage => PageNumber == PageCount - 1;

    // Constructor
    public PagedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        AddRange(items);  // Agregar los elementos a la lista
    }

    // Método estático para crear la lista paginada
    public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }
}