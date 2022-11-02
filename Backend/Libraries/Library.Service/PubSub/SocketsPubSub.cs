
using Library.Core.Enums;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record InvalidateUsersRq();

public record NewSessionRq(Guid UserId);

public record RevokeSessionRq(Guid UserId, Guid? AuthToken = null);

public record RefreshUserRq(Guid UserId, string CurrentHash);

public record UpdatedUserFieldRq(Guid UserId, string Property, object Value);

public record UpdatedUserFieldsRq(Guid UserId, Dictionary<string, object> NewValues);

public record UpdatedUserSettingsRq(Guid UserId, object Settings);

public record UpdatedUserPreferencesRq(Guid UserId, object Preferences);

public record NewNotificationRq(Guid UserId, object Notification, UserLevel? UserLevel = null);

public record NewLocationRq(Guid UserId, Guid PictureId, string Name, string Country, decimal Lat, decimal Lng);

public record ModeratedPictureRq(Guid PictureId, bool Approved);

public interface ISocketsPubSub
{
    Task InvalidateUsers();

    Task NewSession(NewSessionRq payload);

    Task RevokeSession(RevokeSessionRq payload);

    Task UpdatedUserField(UpdatedUserFieldRq payload);

    Task UpdatedUserFields(UpdatedUserFieldsRq payload);

    Task UpdatedUserSettings(UpdatedUserSettingsRq payload);

    Task UpdatedUserPreferences(UpdatedUserPreferencesRq payload);

    Task NewNotification(NewNotificationRq payload);

    Task NewLocation(NewLocationRq payload);

    Task ModeratedPicture(ModeratedPictureRq payload);
}

public class SocketsPubSub : BasePubSub, ISocketsPubSub
{
    public SocketsPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task InvalidateUsers() => await Publish(new InvalidateUsersRq());

    public async Task NewSession(NewSessionRq payload) => await Publish(payload);

    public async Task RevokeSession(RevokeSessionRq payload) => await Publish(payload);

    public async Task UpdatedUserField(UpdatedUserFieldRq payload) => await UpdatedUserFields(new(payload.UserId, new() { { payload.Property, payload.Value } }));

    public async Task UpdatedUserFields(UpdatedUserFieldsRq payload) => await Publish(payload);

    public async Task UpdatedUserSettings(UpdatedUserSettingsRq payload) => await Publish(new UpdatedUserSettingsRq(payload.UserId, payload.Settings));

    public async Task UpdatedUserPreferences(UpdatedUserPreferencesRq payload) => await Publish(payload);

    public async Task NewNotification(NewNotificationRq payload) => await Publish(payload);

    public async Task NewLocation(NewLocationRq payload) => await Publish(payload);

    public async Task ModeratedPicture(ModeratedPictureRq payload) => await Publish(payload);

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
    }
}