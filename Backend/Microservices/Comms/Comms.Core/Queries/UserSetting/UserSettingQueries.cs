
using Comms.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Comms.Core.Queries;

public class UserSettingQueries : IUserSettingQueries
{
    private readonly CommsContext _ctx;

    public UserSettingQueries(CommsContext ctx)
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