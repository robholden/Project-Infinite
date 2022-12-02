using AutoMapper;

using Comms.Core.Services;
using Comms.Domain;
using Comms.Domain.Dtos;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class UpdateGeneralNotificationConsumer : ISnowConsumer, IConsumer<UpdateGeneralNotificationRq>
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

        // Ensure there's no notification with the new key info
        var exists = await _ctx.Notifications.AnyAsync(x => x.UserLevel == request.UserLevel && x.Type == request.NewType && x.ContentKey == request.NewContent.Key);
        if (exists)
        {
            return;
        }

        // Create new notification
        notification.Type = request.NewType;
        notification.ContentKey = request.NewContent.Key;
        notification.ContentMessage = request.NewContent.Message;
        notification.ContentImage = request.NewContent.Image;
        notification.ViewedAt = DateTime.UtcNow;
        notification.Date = DateTime.UtcNow;

        notification = await _ctx.UpdateAsync(notification);

        // Send update if not viewed
        await _service.TryToSend(notification, _mapper.Map<NotificationDto>(notification));
    }
}
