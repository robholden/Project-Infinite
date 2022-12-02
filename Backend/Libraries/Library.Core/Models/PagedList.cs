
using AutoMapper;

using Microsoft.EntityFrameworkCore;

namespace Library.Core;

public enum OrderByDirection
{
    Asc,
    Desc
}

public class PagedList<T>
{
    public PagedList()
    {
    }

    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Rows = items;
        TotalRows = count;
    }

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public IEnumerable<T> Rows { get; init; }

    public int TotalPages { get; init; }

    public int TotalRows { get; init; }

    public string Timestamp { get; init; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();

        if (pageSize > 0)
        {
            source = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        return new PagedList<T>(await source.ToListAsync(), count, pageNumber, pageSize);
    }

    public PagedList<O> ToDto<O>(IMapper mapper) => new()
    {
        PageNumber = PageNumber,
        PageSize = PageSize,
        Rows = mapper.Map<IEnumerable<O>>(Rows),
        TotalPages = TotalPages,
        TotalRows = TotalRows,
        Timestamp = DateTime.UtcNow.ToString()
    };

    public PagedList<O> ToDto<O>(Func<IEnumerable<T>, IEnumerable<O>> mapper) => new()
    {
        PageNumber = PageNumber,
        PageSize = PageSize,
        Rows = mapper.Invoke(Rows),
        TotalPages = TotalPages,
        TotalRows = TotalRows,
        Timestamp = DateTime.UtcNow.ToString()
    };
}