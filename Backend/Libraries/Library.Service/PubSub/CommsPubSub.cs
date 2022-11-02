
using Library.Core.Enums;
using Library.Core.Models;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record AddCommsUserRq(UserRecord User, string Name, string Email, bool Marketing);

public record SendConfirmationEmailToUserRq(UserRecord User, string ConfirmationKey);

public record SendEmailDirectlyRq(string Name, string Email, string Message, string Subject, string IdentityHash = "");

public record SendEmailToUserRq(UserRecord User, string Message, string Subject, EmailType Type, string IdentityHash = "");

public record UpdateEmailTypeRq(string IdentityHash, EmailType Type);

public record SendSmsToUserRq(UserRecord User, string Mobile, string Message, SmsType Type);

public record AddNotificationRq(UserRecord User, NotificationType Type, NotificationContent Content, UserRecord TriggeredUser = null);

public record AddGeneralNotificationRq(UserLevel UserLevel, NotificationType Type, NotificationContent Content);

public record UpdateGeneralNotificationRq(UserLevel UserLevel, NotificationType Type, string ContentKey, NotificationType NewType, NotificationContent NewContent);

public record RemoveNotificationRq(Guid UserId, NotificationType Type, string ContentKey, Guid TriggeredUserId);

public record DeleteNotificationsRq(IEnumerable<string> Keys);

public record OptOutUserRq(Guid UserId);

public interface ICommsPubSub
{
    Task AddUser(AddCommsUserRq payload);

    Task SendConfirmationEmailToUser(SendConfirmationEmailToUserRq payload);

    Task SendEmailDirectly(SendEmailDirectlyRq payload);

    Task SendEmailToUser(SendEmailToUserRq payload);

    Task UpdateEmailType(UpdateEmailTypeRq payload);

    Task OptOutUser(OptOutUserRq payload);

    Task SendSmsToUser(SendSmsToUserRq payload);

    Task AddNotification(AddNotificationRq payload);

    Task AddGeneralNotification(AddGeneralNotificationRq payload);

    Task UpdateGeneralNotification(UpdateGeneralNotificationRq payload);

    Task RemoveNotification(RemoveNotificationRq payload);

    Task DeleteNotification(string key);

    Task DeleteNotifications(DeleteNotificationsRq payload);
}

public class CommsPubSub : BasePubSub, ICommsPubSub
{
    public CommsPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task AddUser(AddCommsUserRq payload) => await PublishWithResult(payload);

    public async Task SendConfirmationEmailToUser(SendConfirmationEmailToUserRq payload) => await Publish(payload);

    public async Task SendEmailDirectly(SendEmailDirectlyRq payload) => await Publish(payload);

    public async Task SendEmailToUser(SendEmailToUserRq payload) => await Publish(payload);

    public async Task UpdateEmailType(UpdateEmailTypeRq payload) => await Publish(payload);

    public async Task OptOutUser(OptOutUserRq payload) => await Publish(payload);

    public async Task SendSmsToUser(SendSmsToUserRq payload) => await Publish(payload);

    public async Task AddNotification(AddNotificationRq payload) => await Publish(payload);

    public async Task AddGeneralNotification(AddGeneralNotificationRq payload) => await Publish(payload);

    public async Task UpdateGeneralNotification(UpdateGeneralNotificationRq payload) => await Publish(payload);

    public async Task RemoveNotification(RemoveNotificationRq payload) => await Publish(payload);

    public async Task DeleteNotification(string key) => await DeleteNotifications(new(new List<string>() { key }));

    public async Task DeleteNotifications(DeleteNotificationsRq payload) => await Publish(payload);

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
        configurator.AddRequestClient<AddCommsUserRq>();
    }
}