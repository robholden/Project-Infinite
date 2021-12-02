
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class UserSettingQueries : IUserSettingQueries
{
    private readonly ContentContext _ctx;

    public UserSettingQueries(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public Task<UserSetting> Get(Guid id)
    {
        return _ctx.UserSettings
            .AsNoTracking()
            .FindOrNullAsync(u => u.UserId == id);
    }
}