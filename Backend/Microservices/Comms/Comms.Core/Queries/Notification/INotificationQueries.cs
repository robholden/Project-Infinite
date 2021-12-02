
using Comms.Domain;

using Library.Core;

namespace Comms.Core.Queries;

public interface INotificationQueries
{
    Task<Notification> Get(Guid id);

    Task<bool> ExistsForUser(Guid userId, Guid id);

    Task<PagedList<Notification>> Lookup(IPagedListRequest<NotificationQueryOptions> pageRequest);

    IEnumerable<Notification> Search(NotificationQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc);

    Task<int> Count(NotificationQueryOptions options);
}