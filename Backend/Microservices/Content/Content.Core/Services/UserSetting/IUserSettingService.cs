
using Content.Domain;

namespace Content.Core.Services;

public interface IUserSettingService
{
    Task<UserSetting> AddOrUpdate(Guid userId, UserSetting userData);
}