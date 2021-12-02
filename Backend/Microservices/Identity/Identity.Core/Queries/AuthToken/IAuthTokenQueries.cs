
using Identity.Domain;

using Library.Core;

namespace Identity.Core.Queries;

public interface IAuthTokenQueries
{
    Task<AuthToken> Get(Guid authTokenId);

    Task<AuthToken> Get(Guid userId, string identityKey, string platform);

    Task<PagedList<AuthToken>> Lookup(IPagedListRequest<AuthTokenQueryOptions> pageRequest);
}