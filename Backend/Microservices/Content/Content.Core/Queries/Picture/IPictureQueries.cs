
using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public interface IPictureQueries
{
    Task<Picture> Get(Guid id, Guid? userId);

    Task<bool> IsAccessible(Guid pictureId);

    Task<bool> BelongsToUser(Guid userId, Guid pictureId);

    Task<IEnumerable<Picture>> NearBy(Guid pictureId);

    Task<IEnumerable<Picture>> Matches(Guid pictureId, int limit = 20);

    Task<PagedList<Picture>> Featured(int page = 1, int pageSize = 10);

    IDictionary<Guid, bool> LikesMap(IEnumerable<Guid> pictureIds, Guid userId);

    Task<bool> Likes(Guid pictureId, Guid userId);

    Task<PagedList<Picture>> Lookup(IPagedListRequest<PictureQueryOptions> pageRequest);

    IEnumerable<Picture> Search(PictureQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc);

    Task<int> Count(PictureQueryOptions options);
}