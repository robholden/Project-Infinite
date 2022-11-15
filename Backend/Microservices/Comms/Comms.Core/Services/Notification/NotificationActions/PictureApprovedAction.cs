using Comms.Domain;

namespace Comms.Core.Services;

public class PictureApprovedAction : NotificationAction, IEmailAction
{
    public PictureApprovedAction(Notification notification) : base(notification)
    {
    }

    public EmailContent GetContent(EmailSettings settings, UserSetting userSetting)
    {
        // Only send when setting enabled
        if (!userSetting.PictureApprovedEmail) return null;

        var sub = "Your picture is live!";
        var body = $"<a href='{settings.WebUrl}/picture/{Notification.ContentKey}'>{Notification.ContentMessage}</a> has been approved by our team and is now live";

        return new(sub, body);
    }
}

