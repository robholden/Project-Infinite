using System.Linq.Expressions;

using Identity.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Queries;

public class AuthTokenQueries : IAuthTokenQueries
{
    private readonly IdentityContext _ctx;

    public AuthTokenQueries(IdentityContext ctx)
    {
        _ctx = ctx;
    }

    public Task<AuthToken> Get(Guid authTokenId)
    {
        return _ctx.AuthTokens
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Preferences)
            .FindOrNullAsync(t => t.AuthTokenId == authTokenId);
    }

    public Task<AuthToken> Get(Guid userId, string identityKey, string platform)
    {
        return _ctx.AuthTokens
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Preferences)
            .FindOrNullAsync(t =>
                t.UserId == userId &&
                t.IdentityKey == identityKey &&
                t.PlatformRaw == platform &&
                !t.Deleted
            );
    }

    public Task<PagedList<AuthToken>> Lookup(IPagedListRequest<AuthTokenQueryOptions> pageRequest)
    {
        // Build where clause
        Expression<Func<AuthToken, bool>> where = x => !x.Deleted;

        // By user
        if (pageRequest.Options.UserId.HasValue)
        {
            where = where.And(x => x.UserId == pageRequest.Options.UserId.Value);
        }

        // Active date (last updated)
        if (pageRequest.Options.ActiveDate.HasValue)
        {
            where = where.And(x => x.Updated.HasValue && x.Updated.Value.Date == pageRequest.Options.ActiveDate.Value.Date);
        }

        // Active
        if (pageRequest.Options.Active.HasValue)
        {
            where = where.And(x => x.Active);
        }

        // Ip address
        if (!string.IsNullOrEmpty(pageRequest.Options.IpAddress))
        {
            where = where.And(x => x.IpAddress.StartsWith(pageRequest.Options.IpAddress.ToLower().Trim()));
        }

        // Ignore an access key?
        if (pageRequest.Options.IgnoreAccessKey.HasValue)
        {
            where = where.And(x => x.AuthTokenId != pageRequest.Options.IgnoreAccessKey);
        }

        // Return in given order
        Expression<Func<AuthToken, object>> orderBy = pageRequest.Options.OrderBy switch
        {
            AuthTokenQueryOptions.OrderByEnum.CreationDate => x => x.Created,
            AuthTokenQueryOptions.OrderByEnum.RefreshedDate => x => x.RefreshedAt,
            _ => x => x.Updated,
        };

        return _ctx.AuthTokens
            .AsNoTracking()
            .QueryAsync(where, pageRequest, orderBy);
    }
}