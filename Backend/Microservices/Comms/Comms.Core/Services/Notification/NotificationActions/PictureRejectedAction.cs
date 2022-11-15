using Comms.Domain;

namespace Comms.Core.Services;

public class PictureRejectedAction : NotificationAction, IEmailAction
{
    public PictureRejectedAction(Notification notification) : base(notification)
    {
    }

    public EmailContent GetContent(EmailSettings settings, UserSetting userSetting)
    {
        // Only send when setting enabled
        if (!userSetting.PictureUnapprovedEmail) return null;

        var sub = "Sorry, we couldn't accept your picture";
        var body = $"<b>{Notification.ContentMessage}</b> did not meet our standards or criteria";

        return new(sub, body);
    }
}

