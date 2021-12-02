
using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public interface IPictureModerationQueries
{
    Task<PagedList<PictureModeration>> Lookup(string email, IPagedListRequest<PictureModerationQueryOptions> pageRequest);
}