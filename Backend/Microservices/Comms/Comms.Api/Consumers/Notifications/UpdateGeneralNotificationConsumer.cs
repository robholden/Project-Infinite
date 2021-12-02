using AutoMapper;

using Comms.Api.Dtos;
using Comms.Core.Services;
using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Notifications;

public class UpdateGeneralNotificationConsumer: ISnowConsumer, IConsumer<UpdateGeneralNotificationRq>
{
    private readonly IMapper _mapper;
    private readonly CommsContext _ctx;

    private readonly INotificationService _service;

    public UpdateGeneralNotificationConsumer(IMapper mapper, CommsContext ctx, INotificationService service)
    {
        _mapper = mapper;
        _ctx = ctx;

        _service = service;
    }

    public async Task Consume(ConsumeContext<UpdateGeneralNotificationRq> context)
    {
        // Make sure the one old exists
        var request = context.Message;
        var notification = await _ctx.Notifications.FindOrNullAsync(x => x.UserLevel == request.UserLevel && x.Type == request.Type && x.ContentKey == request.ContentKey);
        if (notification == null)
        {
            return;
        }

        // Find existing notification
        var existing = await _ctx.Notifications.FindOrNullAsync(x => x.UserLevel == request.UserLevel && x.Type == request.NewType && x.ContentKey == request.NewContent.Key);
        if (existing != null)
        {
            return;
        }

        // Create new notification
        notification.Type = request.NewType;
        notification.ContentMessage = request.NewContent.Message;
        notification.ContentImage = request.NewContent.Image;
        notification.ViewedAt = DateTime.UtcNow;
        notification.Date = DateTime.UtcNow;

        notification = await _ctx.Put(notification);

        // Send update if not viewed
        await _service.TryToSend(notification, _mapper.Map<NotificationDto>(notification));
    }
}
