using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class RemoveNotificationConsumer: ISnowConsumer, IConsumer<RemoveNotificationRq>
{
    private readonly CommsContext _ctx;

    public RemoveNotificationConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<RemoveNotificationRq> context)
    {
        // Find the associated notification, exit if not found
        var request = context.Message;
        var notification = await _ctx.Notifications
           .Include(x => x.Entries)
           .Where(x => x.Entries.Any(e => e.UserId == request.UserId && !e.Deleted))
           .FirstOrDefaultAsync(x => x.UserId == request.TriggeredUserId && x.Type == request.Type && x.ContentKey == request.ContentKey);

        if (notification == null)
        {
            return;
        }

        // Find and set entry as deleted
        var entry = notification.Entries.FirstOrDefault(x => x.UserId == request.UserId && !x.Deleted);
        entry.Deleted = true;
        await _ctx.Put(entry);

        // If there is only one un-deleted entry (the one we just marked as deleted) then set the notification to hidden
        notification.Hidden = notification.Entries.Count(x => x.Deleted) == 1;
        await _ctx.Put(notification);
    }
}
