using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class DeleteNotificationsConsumer : ISnowConsumer, IConsumer<DeleteNotificationsRq>
{
    private readonly CommsContext _ctx;

    public DeleteNotificationsConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<DeleteNotificationsRq> context)
    {
        // Find the associated notification, exit if not found
        var notifications = _ctx.Notifications.Where(n => context.Message.Keys.Contains(n.ContentKey) && !n.UserLevel.HasValue);
        if (notifications.Any())
        {
            await _ctx.DeleteRange(notifications);
        }
    }
}
