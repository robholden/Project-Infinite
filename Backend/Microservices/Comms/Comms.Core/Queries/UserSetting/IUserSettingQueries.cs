
using Comms.Domain;

namespace Comms.Core.Queries;

public interface IUserSettingQueries
{
    Task<UserSetting> Get(Guid id);
}