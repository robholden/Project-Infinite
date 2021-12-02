using System.Linq.Expressions;

using Identity.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Queries;

public class UserQueries : IUserQueries
{
    private readonly IdentityContext _ctx;

    public UserQueries(IdentityContext ctx)
    {
        _ctx = ctx;
    }

    public Task<User> Get(Guid id)
    {
        return _ctx.Users
            .AsNoTracking()
            .FindOrNullAsync(u => u.UserId == id);
    }

    public Task<User> GetByUsername(string value)
    {
        value = (value ?? "").ToLower();
        return _ctx.Users
            .AsNoTracking()
            .FindOrNullAsync(u => u.Username.ToLower() == value);
    }

    public Task<User> GetByEmail(string value)
    {
        value = (value ?? "").ToLower();
        return _ctx.Users
            .AsNoTracking()
            .FindOrNullAsync(u => u.Email.ToLower() == value);
    }

    public Task<PagedList<User>> Lookup(IPagedListRequest<UserQueryOptions> pageRequest)
    {
        // Build where clause
        Expression<Func<User, bool>> where = _ => true;

        // By User Id
        if (pageRequest.Options.UserId.HasValue)
        {
            where = where.And(x => x.UserId == pageRequest.Options.UserId.Value);
        }

        // Active
        if (pageRequest.Options.Active.HasValue)
        {
            where = where.And(x => x.Status == UserStatus.Enabled);
        }

        // Email address
        if (!string.IsNullOrEmpty(pageRequest.Options.EmailAddress))
        {
            where = where.And(x => x.Email.StartsWith(pageRequest.Options.EmailAddress.ToLower().Trim()));
        }

        // Name
        if (!string.IsNullOrEmpty(pageRequest.Options.Name))
        {
            where = where.And(x => x.Name.StartsWith(pageRequest.Options.Name.ToLower().Trim()));
        }

        // Username
        if (!string.IsNullOrEmpty(pageRequest.Options.Username))
        {
            where = where.And(x => x.Email.StartsWith(pageRequest.Options.Username.ToLower().Trim()));
        }

        // Return in given order
        Expression<Func<User, object>> orderBy = null;
        switch (pageRequest.Options.OrderBy)
        {
            case UserQueryOptions.OrderByEnum.CreationDate:
                orderBy = x => x.Created;
                break;

            case UserQueryOptions.OrderByEnum.Email:
                orderBy = x => x.Email;
                break;

            case UserQueryOptions.OrderByEnum.LastActive:
                orderBy = x => x.LastActive;
                break;

            case UserQueryOptions.OrderByEnum.Name:
                orderBy = x => x.Name;
                break;

            case UserQueryOptions.OrderByEnum.Username:
                orderBy = x => x.Username;
                break;
        }

        return _ctx.Users
            .AsNoTracking()
            .QueryAsync(where, pageRequest, orderBy);
    }
}