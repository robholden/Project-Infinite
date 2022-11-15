using Comms.Domain;

using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class UndoUserNotificationConsumer : ISnowConsumer, IConsumer<UndoUserNotificationRq>
{
    private readonly CommsContext _ctx;

    public UndoUserNotificationConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UndoUserNotificationRq> context)
    {
        // Mark entry as deleted
        var request = context.Message;
        await _ctx.NotificationEntries
            .Where(x =>
                x.Notification.UserId == request.TriggeredUserId && x.Notification.Type == request.Type && x.Notification.ContentKey == request.ContentKey
                && x.UserId == request.UserId && !x.Deleted
            )
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.Deleted, true));

        // If notification has no visible children, hide it
        await _ctx.Notifications
            .Where(x =>
                x.UserId == request.TriggeredUserId && x.Type == request.Type && x.ContentKey == request.ContentKey
                && x.Entries.All(e => e.Deleted)
            )
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.Hidden, true));
    }
}
