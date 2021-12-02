
using Content.Domain;

namespace Content.Core.Queries;

public interface IUserSettingQueries
{
    Task<UserSetting> Get(Guid id);
}