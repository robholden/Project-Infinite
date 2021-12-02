
using Comms.Domain;

namespace Comms.Core.Services;

public interface IUserSettingService
{
    Task Unsubscribe(Guid key);

    Task Update(Guid userId, UserSetting userData);
}