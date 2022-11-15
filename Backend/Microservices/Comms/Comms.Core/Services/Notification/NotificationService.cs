
using Comms.Domain;

using Library.Core;
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
        await _ctx.Notifications
            .Where(x => x.UserId == userId && !x.ReadAt.HasValue && x.UserLevel == level)
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.ReadAt, DateTime.UtcNow));
    }

    public async Task MarkAsRead(Guid id)
    {
        var notification = await _ctx.Notifications.FindAsync(x => x.NotificationId == id);

        notification.ReadAt = DateTime.UtcNow;
        await _ctx.UpdateAsync(notification);
    }

    public async Task Viewed(Guid userId, UserLevel? level = null)
    {
        await _ctx.Notifications
            .Where(x => x.UserId == userId && !x.ViewedAt.HasValue && x.UserLevel == level)
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.ViewedAt, DateTime.UtcNow));
    }

    public async Task Delete(Guid id)
    {
        await _ctx.Notifications.Where(x => x.NotificationId == id).ExecuteDeleteAsync();
    }

    public async Task<bool> TryToSend(Notification notification, object payload)
    {
        // Get action
        var action = NotificationAction.Create(notification);
        if (action == null) return false;

        // Delay sending if required
        var delay = NotificationAction.DelayBeforeSending();
        if (delay > 0)
        {
            await Task.Delay(delay);

            var stillExists = await _ctx.Notifications.AnyAsync(x => x.NotificationId == notification.NotificationId && !x.Hidden);
            if (!stillExists) return false;
        }

        // Send ui update
        _ = _socketEvents?.NewNotification(new(notification.UserId, payload, notification.UserLevel));

        // Send email to user
        if (notification.Username != null)
        {
            await EmailNotification(action);
        }

        return true;
    }

    private async Task EmailNotification(NotificationAction action)
    {
        // Stop if action doesn't require emailing
        if (action is not IEmailAction emailAction) return;

        // If action is stacked, update entries to sent
        if (action is IStackedNotification stacked)
        {

            // Assign notification as sent to prevent re-sending
            var affected = await _ctx.NotificationEntries
                .Where(x => x.NotificationId == action.Notification.NotificationId && !x.Sent)
                .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.Sent, true));

            // If no notifications were affected, stop
            if (!stacked.TrySetAffected(affected)) return;
        }

        // Fetch user preferences
        var (preferences, _) = await _ctx.UserSettings.FindWithDefaultAsync(x => x.UserId == action.Notification.UserId, new());

        // Get email content from action
        var content = emailAction.GetContent(_emailSettings, preferences);

        // Send email when we have content
        if (content != null)
        {
            var request = new SendEmailToUserRq(action.Notification.ToUserRecord(), content.Body, content.Subject, EmailType.System, $"{action.Notification.ContentKey}_{action.Notification.Type}");
            _ = _commEvents?.SendEmailToUser(request);
        }
    }
}