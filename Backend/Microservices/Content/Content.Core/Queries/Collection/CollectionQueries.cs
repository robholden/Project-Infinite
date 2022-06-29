using System.Linq.Expressions;

using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class CollectionQueries : ICollectionQueries
{
    private readonly ContentContext _ctx;

    public CollectionQueries(ContentContext context)
    {
        _ctx = context;
    }

    public Task<Collection> Get(Guid id)
    {
        return _ctx.Collections
            .AsNoTracking()
            .Include(x => x.Pictures)
            .FirstOrDefaultAsync(c => c.CollectionId == id);
    }

    public Task<bool> ExistsForUser(Guid userId, Guid id)
    {
        return _ctx.Collections
            .AsNoTracking()
            .AnyAsync(c => c.CollectionId == id && c.UserId == userId);
    }

    public async Task<PagedList<Collection>> Lookup(IPagedListRequest<CollectionQueryOptions> pageRequest)
    {
        // Build where clause
        var where = BuildExpression(pageRequest.Options, out var orderBy);

        return await _ctx.Collections
            .AsNoTracking()
            .Include(x => x.Pictures.Where(x => x.PictureId == (pageRequest.Options.IncludePictureId ?? x.PictureId)).Take(9))
            .QueryAsync(where, pageRequest, orderBy);
    }

    public IEnumerable<Collection> Search(CollectionQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc)
    {
        // Build where clause
        var where = BuildExpression(options, out var orderBy);

        var enumer = _ctx.Collections
            .Include(c => c.Pictures.Where(x => x.PictureId == (options.IncludePictureId ?? x.PictureId)).Take(9))
            .AsNoTracking()
            .Where(where);

        if (orderBy == null) return enumer;
        else if (orderDir == OrderByDirection.Asc) return enumer.OrderBy(orderBy);
        else return enumer.OrderByDescending(orderBy);
    }

    public Task<int> Count(CollectionQueryOptions options)
    {
        // Build where clause
        var where = BuildExpression(options, out _);

        return _ctx.Collections.AsNoTracking().CountAsync(where);
    }

    private static Expression<Func<Collection, bool>> BuildExpression(CollectionQueryOptions options, out Expression<Func<Collection, object>> orderBy)
    {
        // Default options
        options ??= new CollectionQueryOptions();

        // Build where clause
        Expression<Func<Collection, bool>> where = _ => true;

        // User Id
        if (options.UserId.HasValue)
        {
            where = where.And(x => x.UserId == options.UserId.Value);
        }

        // Username
        if (!string.IsNullOrEmpty(options.Username))
        {
            where = where.And(x => x.Username == options.Username);
        }

        // Name
        if (!string.IsNullOrEmpty(options.Name))
        {
            where = where.And(x => x.Name.ToLower().StartsWith(options.Name.ToLower().Trim()));
        }

        orderBy = options.OrderBy switch
        {
            CollectionQueryOptions.OrderByEnum.Name => x => x.Name,
            _ => x => x.Ordinal,
        };
        return where;
    }
}