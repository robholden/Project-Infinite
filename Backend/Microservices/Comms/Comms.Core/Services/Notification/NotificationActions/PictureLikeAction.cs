using Comms.Domain;

namespace Comms.Core.Services;

public class PictureLikeAction : NotificationAction, IEmailAction, IStackedNotification
{
    private int _affected;

    public PictureLikeAction(Notification notification) : base(notification)
    {
    }

    public static new int DelayBeforeSending() => 5000;

    public EmailContent GetContent(EmailSettings settings, UserSetting userSetting)
    {
        // Only send when setting enabled
        if (!userSetting.PictureLikedEmail) return null;

        string body;
        string sub;

        if (_affected > 1)
        {
            sub = $"{_affected} have liked your picture!";
            body = $"<b>{_affected}</b> have liked your picture <a href='{settings.WebUrl}/picture/{Notification.ContentKey}'>{Notification.ContentMessage}</a>!";
        }
        else
        {
            sub = "Someone has liked your picture!";
            body = $"<b>{Notification.Entries.FirstOrDefault()?.Username ?? "Someone"}</b> has liked your picture <a href='{settings.WebUrl}/picture/{Notification.ContentKey}'>{Notification.ContentMessage}</a>!";
        }

        return new(sub, body);
    }

    public bool TrySetAffected(int affected)
    {
        _affected = affected;
        return affected > 0;

    }
}

