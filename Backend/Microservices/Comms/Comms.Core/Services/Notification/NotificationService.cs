
using Comms.Domain;

using Library.Core;
using Library.Core.Enums;
using Library.Core.Models;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Comms.Core.Services;

public class NotificationService : INotificationService
{
    private readonly CommsContext _ctx;

    private readonly EmailSettings _emailSettings;

    private readonly ISocketsPubSub _socketEvents;
    private readonly ICommsPubSub _commEvents;

    public NotificationService(CommsContext ctx, IOptions<EmailSettings> emailOptions, ICommsPubSub commEvents, ISocketsPubSub socketEvents)
    {
        _ctx = ctx;

        _emailSettings = emailOptions.Value;

        _commEvents = commEvents;
        _socketEvents = socketEvents;
    }

    public async Task MarkAllAsRead(Guid userId, UserLevel? level = null)
    {
        var notifications = _ctx.Notifications.Where(x => x.UserId == userId && !x.ReadAt.HasValue && x.UserLevel == level);
        await notifications.ForEachAsync(x => x.ReadAt = DateTime.UtcNow);

        await _ctx.PutRange(notifications);
    }

    public async Task MarkAsRead(Guid id)
    {
        var notification = await _ctx.Notifications.FindAsync(x => x.NotificationId == id);

        notification.ReadAt = DateTime.UtcNow;
        await _ctx.Put(notification);
    }

    public async Task Viewed(Guid userId, UserLevel? level = null)
    {
        var now = DateTime.UtcNow;
        var notifications = _ctx.Notifications.Where(n => n.UserId == userId && !n.ViewedAt.HasValue && n.UserLevel == level);
        await notifications.ForEachAsync(n => n.ViewedAt = now);

        await _ctx.PutRange(notifications);
    }

    public async Task Delete(Guid id)
    {
        var notification = await _ctx.Notifications.FindOrNullAsync(n => n.NotificationId == id);
        if (notification == null)
        {
            return;
        }

        await _ctx.Delete(notification);
    }

    public async Task<bool> TryToSend<T>(Notification notification, T notificationDto)
    {
        // Wait 5 seconds, just in case they've undone their action for likes
        if (notification.Type == NotificationType.NewLike)
        {
            await Task.Delay(5000);
            if (!_ctx.Notifications.Any(x => x.NotificationId == notification.NotificationId && !x.Hidden))
            {
                return false;
            }
        }

        // Send ui update
        _ = (_socketEvents?.NewNotification(new(notification.UserId, notificationDto, notification.UserLevel)));

        // Send email to user
        await Send(notification);

        return true;
    }

    public async Task Send(Notification notification)
    {
        // If there are no entries, stop
        var entries = _ctx.NotificationEntries.Where(x => x.NotificationId == notification.NotificationId && !x.Sent);
        var totalEntries = await entries.CountAsync();
        if (totalEntries == 0)
        {
            return;
        }

        // Get latest version of the notification
        notification = await _ctx.Notifications.FindAsync(x => x.NotificationId == notification.NotificationId);

        // Fetch user preferences
        var preferences = await _ctx.UserSettings.FindAsync(x => x.UserId == notification.UserId);

        // Set all entries as sent to prevent re-sending
        await entries.ForEachAsync(x => x.Sent = true);
        await _ctx.PutRange(entries);

        // Ensure we can send email for the type of notification
        var emailSubject = string.Empty;
        var emailContent = string.Empty;

        if (notification.Type == NotificationType.NewLike && preferences.PictureLikedEmail)
        {
            if (totalEntries > 1)
            {
                emailSubject = $"{ totalEntries } have liked your picture!";
                emailContent = $"<b>{ totalEntries }</b> have liked your picture <a href='{ _emailSettings.WebUrl }/picture/{ notification.ContentKey }'>{ notification.ContentMessage }</a>!";
            }
            else
            {
                emailSubject = "Someone has liked your picture!";
                emailContent = $"<b>{ notification.Entries.FirstOrDefault()?.Username ?? "Someone" }</b> has liked your picture <a href='{ _emailSettings.WebUrl }/picture/{ notification.ContentKey }'>{ notification.ContentMessage }</a>!";
            }
        }
        else if (notification.Type == NotificationType.PictureApproved && preferences.PictureApprovedEmail)
        {
            emailSubject = "Your picture is live!";
            emailContent = $"<a href='{ _emailSettings.WebUrl }/picture/{ notification.ContentKey }'>{ notification.ContentMessage }</a> has been approved by our team and is now live";
        }
        else if (notification.Type == NotificationType.PictureUnapproved && preferences.PictureUnapprovedEmail)
        {
            emailSubject = "Sorry, we couldn't accept your picture";
            emailContent = $"<b>{ notification.ContentMessage }</b> did not meet our standards or criteria";
        }

        // Send email when we have content
        if (emailSubject != string.Empty && emailContent != string.Empty)
        {
            var request = new SendEmailToUserRq(notification.ToUserRecord(), emailContent, emailSubject, EmailType.System, $"{ notification.ContentKey }_{ notification.Type }");
            _ = _commEvents?.SendEmailToUser(request);
        }
    }
}