using Comms.Domain;

using Library.Core;

namespace Comms.Core.Services;

public record EmailContent(string Subject, string Body);

public interface IEmailAction
{
    EmailContent GetContent(EmailSettings settings, UserSetting userSetting);
}

public interface IStackedNotification
{
    bool TrySetAffected(int affected);
}

public abstract class NotificationAction
{
    public Notification Notification { get; private set; }

    public NotificationAction(Notification notification)
    {
        Notification = notification;
    }

    public static int DelayBeforeSending() => 0;

    public static NotificationAction Create(Notification notification) => notification.Type switch
    {
        NotificationType.NewLike => new PictureLikeAction(notification),
        NotificationType.PictureApproved => new PictureApprovedAction(notification),
        _ => null,
    };
}

