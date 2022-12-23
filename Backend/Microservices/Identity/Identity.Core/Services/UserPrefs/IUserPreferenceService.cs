namespace Identity.Core.Services;

public interface IUserPreferenceService
{
    Task SetMarketing(Guid userId, bool enabled);

    Task UnsubscribeFromMarketing(Guid optOutKey);
}