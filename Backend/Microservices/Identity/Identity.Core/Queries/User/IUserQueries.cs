
using Identity.Domain;

using Library.Core;

namespace Identity.Core.Queries;

public interface IUserQueries
{
    Task<User> Get(Guid id);

    Task<User> GetByEmail(string value);

    Task<User> GetByUsername(string value);

    Task<PagedList<User>> Lookup(IPagedListRequest<UserQueryOptions> pageRequest);
}