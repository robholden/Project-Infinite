using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public interface IPostQueries
{
    Task<Post> Get(Guid id);

    Task<bool> DoesBelongToUser(Guid postId, Guid userId);

    Task<PagedList<Post>> Lookup(IPagedListRequest<PostQueryOptions> pageRequest);
}