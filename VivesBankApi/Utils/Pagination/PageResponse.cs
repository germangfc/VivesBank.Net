public class PageResponse<T>
{
    public List<T> Content { get; set; }
    public int TotalPages { get; set; }
    public long TotalElements { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalPageElements { get; set; }
    public bool Empty { get; set; }
    public bool First { get; set; }
    public bool Last { get; set; }
    public string SortBy { get; set; }
    public string Direction { get; set; }

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