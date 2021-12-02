using System.Linq.Expressions;

using Comms.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Comms.Core.Queries;

public class NotificationQueries : INotificationQueries
{
    private readonly CommsContext _ctx;

    public NotificationQueries(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public Task<Notification> Get(Guid id)
    {
        return _ctx.Notifications
            .AsNoTracking()
            .Include(x => x.Entries.Where(x => !x.Deleted))
            .FindOrNullAsync(n => n.NotificationId == id);
    }

    public Task<bool> ExistsForUser(Guid userId, Guid id)
    {
        return _ctx.Notifications
            .AsNoTracking()
            .AnyAsync(n => n.NotificationId == id && n.UserId == userId);
    }

    public Task<PagedList<Notification>> Lookup(IPagedListRequest<NotificationQueryOptions> pageRequest)
    {
        // Build where clause
        var where = BuildExpression(pageRequest.Options, out var orderBy);

        return _ctx.Notifications
            .AsNoTracking()
            .Include(x => x.Entries.Where(x => !x.Deleted))
            .QueryAsync(where, pageRequest, orderBy);
    }

    public IEnumerable<Notification> Search(NotificationQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc)
    {
        // Build where clause
        var where = BuildExpression(options, out var orderBy);

        var enumer = _ctx.Notifications
            .AsNoTracking()
            .Include(x => x.Entries.Where(x => !x.Deleted))
            .Where(where);

        if (orderBy == null) return enumer;
        else if (orderDir == OrderByDirection.Asc) return enumer.OrderBy(orderBy);
        else return enumer.OrderByDescending(orderBy);
    }

    public Task<int> Count(NotificationQueryOptions options)
    {
        // Build where clause
        var where = BuildExpression(options, out _);

        return _ctx.Notifications
            .AsNoTracking()
            .CountAsync(where);
    }

    private static Expression<Func<Notification, bool>> BuildExpression(NotificationQueryOptions options, out Expression<Func<Notification, object>> orderBy)
    {
        // Build where clause
        orderBy = null;
        Expression<Func<Notification, bool>> where = x => !x.Hidden;

        // User & user level
        if (options.UserId.HasValue && options.UserLevels.Count > 0)
        {
            where = where.And(x =>
                x.UserId == options.UserId.Value ||
                (x.UserId == new Guid() && x.UserLevel.HasValue && options.UserLevels.Contains(x.UserLevel.Value))
            );
        }

        // User
        else if (options.UserId.HasValue)
        {
            where = where.And(x => x.UserId == options.UserId.Value);
        }

        // User Level
        if (options.UserLevels.Count > 0)
        {
            where = where.And(x => x.UserLevel.HasValue && options.UserLevels.Contains(x.UserLevel.Value));
        }

        // Viewed
        if (options.Viewed.HasValue)
        {
            where = where.And(x => x.ViewedAt.HasValue == options.Viewed.Value);
        }

        // Read
        if (options.Read.HasValue)
        {
            where = where.And(x => x.ReadAt.HasValue == options.Read.Value);
        }

        // Type
        if (options.Types.Count > 0)
        {
            where = where.And(x => options.Types.Contains(x.Type));
        }

        // After date
        if (!string.IsNullOrEmpty(options.AfterTimestamp) && DateTime.TryParse(options.AfterTimestamp, out var date))
        {
            where = where.And(x => x.Date > date);
        }

        // Return in given order
        switch (options.OrderBy)
        {
            case NotificationQueryOptions.OrderByEnum.Date:
                orderBy = x => x.Date;
                break;
        }

        return where;
    }
}