
using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public interface ILocationQueries
{
    IEnumerable<Location> GetAll();

    Task<Location> WithinBounds(decimal lat, decimal lng);

    Task<PagedList<Location>> Lookup(IPagedListRequest<LocationQueryOptions> pageRequest);
}