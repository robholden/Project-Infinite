
using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public interface ICollectionQueries
{
    Task<Collection> Get(Guid id);

    Task<bool> ExistsForUser(Guid userId, Guid id);

    Task<PagedList<Collection>> Lookup(IPagedListRequest<CollectionQueryOptions> pageRequest);

    IEnumerable<Collection> Search(CollectionQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc);

    Task<int> Count(CollectionQueryOptions options);
}