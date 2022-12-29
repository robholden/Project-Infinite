using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class UndoUserNotificationConsumer : IRabbitConsumer, IConsumer<UndoUserNotificationRq>
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
        await _ctx.ExecuteUpdateAsync<NotificationEntry>(
            x => x.Notification.UserId == request.TriggeredUserId && x.Notification.Type == request.Type && x.UserId == request.UserId && !x.Deleted,
            entry => entry.Deleted = true
        );

        // If notification has no visible children, hide it
        await _ctx.ExecuteUpdateAsync<Notification>(
             x => x.UserId == request.TriggeredUserId && x.Type == request.Type && x.Identifier == request.Identifier && x.Entries.All(e => e.Deleted),
            entry => entry.Hidden = true
        );
    }
}
