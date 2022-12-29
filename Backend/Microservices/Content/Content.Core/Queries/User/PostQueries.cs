using System.Linq.Expressions;

using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class PostQueries : IPostQueries
{
    private readonly ContentContext _ctx;

    public PostQueries(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public Task<Post> Get(Guid id)
    {
        return _ctx.Posts
            .AsNoTracking()
            .FindOrNullAsync(u => u.PostId == id);
    }

    public Task<bool> DoesBelongToUser(Guid postId, Guid userId)
    {
        return _ctx.Posts
            .AsNoTracking()
            .AnyAsync(p => p.PostId == postId && p.UserId == userId);
    }

    public Task<PagedList<Post>> Lookup(IPagedListRequest<PostQueryOptions> pageRequest)
    {
        // Build where clause
        Expression<Func<Post, bool>> where = _ => true;

        // Username
        if (!string.IsNullOrEmpty(pageRequest.Options.Username))
        {
            where = where.And(x => x.Author.Equals(pageRequest.Options.Username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Filter text
        if (!string.IsNullOrEmpty(pageRequest.Options.FilterText))
        {
            var text = pageRequest.Options.FilterText.Trim();
            var searchTitle = pageRequest.Options.FilterBy?.HasFlag(PostQueryOptions.FilterParams.Title) == true;
            var searchBody = pageRequest.Options.FilterBy?.HasFlag(PostQueryOptions.FilterParams.Body) == true;

            where = where.And(x =>
                (!searchTitle || x.Title.Contains(text, StringComparison.OrdinalIgnoreCase)) &&
                (!searchBody || x.Body.Contains(text, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Return in given order
        Expression<Func<Post, object>> orderBy = null;
        switch (pageRequest.Options.OrderBy)
        {
            case PostQueryOptions.OrderByEnum.CreatedDate:
                orderBy = x => x.Created;
                break;

            case PostQueryOptions.OrderByEnum.UpdatedDate:
                orderBy = x => x.Updated;
                break;

            case PostQueryOptions.OrderByEnum.Username:
                orderBy = x => x.Author;
                break;

            case PostQueryOptions.OrderByEnum.Title:
                orderBy = x => x.Title;
                break;
        }

        return _ctx.Posts
            .AsNoTracking()
            .QueryAsync(where, pageRequest, orderBy);
    }
}