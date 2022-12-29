using Identity.Domain.Dtos;

namespace Identity.Core.Services;

public interface IUserPreferenceService
{
    Task AddOrUpdate(Guid userId, UserPreferencesDto prefs);

    Task UnsubscribeFromMarketing(Guid optOutKey);
}