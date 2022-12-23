
using Library.Core;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record SendEmailDirectlyRq(string Name, string Email, string Message, string Subject, string IdentityHash = "");

public record SendEmailToUserRq(UserRecord User, string Message, string Subject, bool SendInstantly = false, string IdentityHash = "");

public record SendSmsToUserRq(UserRecord User, string Type, string Mobile, string Message);

public record AddNotificationRq(UserRecord User, string Identifier, NotificationType Type, NotificationContent Content = null, UserRecord TriggeredUser = null, SendEmailToUserRq Email = null);

public record AddGeneralNotificationRq(UserLevel UserLevel, string Identifier, NotificationType Type, NotificationContent Content);

public record UpdateGeneralNotificationRq(UserLevel UserLevel, string Identifier, NotificationType Type, NotificationType NewType, NotificationContent NewContent);

public record UndoUserNotificationRq(Guid UserId, string Identifier, NotificationType Type, Guid TriggeredUserId);

public interface ICommsPubSub
{
    Task SendEmailDirectly(SendEmailDirectlyRq payload);

    Task SendEmailToUser(SendEmailToUserRq payload);

    Task SendSmsToUser(SendSmsToUserRq payload);

    Task AddNotification(AddNotificationRq payload);

    Task AddGeneralNotification(AddGeneralNotificationRq payload);

    Task UpdateGeneralNotification(UpdateGeneralNotificationRq payload);

    Task UndoUserNotification(UndoUserNotificationRq payload);
}

public class CommsPubSub : BasePubSub, ICommsPubSub
{
    public CommsPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task SendEmailDirectly(SendEmailDirectlyRq payload) => await Publish(payload);

    public async Task SendEmailToUser(SendEmailToUserRq payload) => await Publish(payload);

    public async Task SendSmsToUser(SendSmsToUserRq payload) => await Publish(payload);

    public async Task AddNotification(AddNotificationRq payload) => await Publish(payload);

    public async Task AddGeneralNotification(AddGeneralNotificationRq payload) => await Publish(payload);

    public async Task UpdateGeneralNotification(UpdateGeneralNotificationRq payload) => await Publish(payload);

    public async Task UndoUserNotification(UndoUserNotificationRq payload) => await Publish(payload);

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
    }
}