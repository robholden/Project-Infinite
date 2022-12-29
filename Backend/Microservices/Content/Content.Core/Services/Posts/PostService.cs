using Content.Domain;
using Content.Domain.Dtos;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Services;

public class PostService : IPostService
{
    private readonly ContentContext _ctx;

    public PostService(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Post> Create(PostDto data, IUser user)
    {
        var post = new Post(user, data);

        post = (await _ctx.Posts.AddAsync(post)).Entity;
        await _ctx.SaveChangesAsync();

        return post;
    }

    public async Task<Post> Update(Guid id, PostDto data)
    {
        var post = await _ctx.Posts.FindAsync(p => p.PostId == id);

        post.Title = data.Title ?? post.Title;
        post.Body = data.Body ?? post.Body;
        post.Updated = DateTime.UtcNow;

        post = _ctx.Posts.Update(post).Entity;
        await _ctx.SaveChangesAsync();

        return post;
    }

    public Task Delete(Guid id) => _ctx.ExecuteDeleteAsync<Post>(p => p.PostId == id);
}
