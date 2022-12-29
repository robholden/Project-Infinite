using Content.Domain;
using Content.Domain.Dtos;

using Library.Core;

namespace Content.Core.Services;

public interface IPostService
{
    Task<Post> Create(PostDto data, IUser user);

    Task<Post> Update(Guid id, PostDto post);

    Task Delete(Guid id);
}
