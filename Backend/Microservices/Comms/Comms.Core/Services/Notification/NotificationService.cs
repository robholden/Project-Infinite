
using Comms.Domain;
using Comms.Domain.Dtos;

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
    private readonly IEmailService _emailService;

    public NotificationService(CommsContext ctx, IOptions<EmailSettings> emailOptions, ISocketsPubSub socketEvents, IEmailService emailService)
    {
        _ctx = ctx;
        _emailSettings = emailOptions.Value;

        _socketEvents = socketEvents;
        _emailService = emailService;
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

    public async Task<bool> TryToSend(Notification notification, NotificationDto dto)
    {
        // Delay sending if required
        if (notification.Delay > 0)
        {
            await Task.Delay(notification.Delay);

            var stillExists = await _ctx.Notifications.AnyAsync(x => x.NotificationId == notification.NotificationId && !x.Hidden);
            if (!stillExists) return false;
        }

        // Send ui update
        _ = _socketEvents?.NewNotification(notification.UserId, dto, notification.UserLevel);

        // Send email to user
        if (notification.EmailQueueId.HasValue)
        {
            await _emailService.Send(notification.EmailQueueId.Value);
        }

        return true;
    }
}