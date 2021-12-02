namespace Library.Core;

public interface IPageListQuery<T>
{
    T OrderBy { get; set; }
}

public interface IPagedListRequest
{
    OrderByDirection? OrderDir { get; }

    int PageNumber { get; set; }

    int PageSize { get; set; }
}

public interface IPagedListRequest<T> : IPagedListRequest
{
    T Options { get; }
}

public class PagedListRequest : IPagedListRequest
{
    public PagedListRequest(int pageNumber, int pageSize, OrderByDirection? orderDir = OrderByDirection.Asc)
    {
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        PageSize = pageSize <= 0 ? 1 : pageSize;
        OrderDir = orderDir;
    }

    public OrderByDirection? OrderDir { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

public class PagedListRequest<T> : PagedListRequest, IPagedListRequest<T>
{
    public PagedListRequest(int pageNumber, int pageSize, OrderByDirection? orderDir = OrderByDirection.Asc, T options = default)
        : base(pageNumber, pageSize, orderDir)
    {
        Options = options;
    }

    public T Options { get; set; }
}