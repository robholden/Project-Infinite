using AutoMapper;

using Comms.Core.Services;
using Comms.Domain;
using Comms.Domain.Dtos;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class AddGeneralNotificationConsumer : IRabbitConsumer, IConsumer<AddGeneralNotificationRq>
{
    private readonly IMapper _mapper;
    private readonly CommsContext _ctx;

    private readonly INotificationService _service;

    public AddGeneralNotificationConsumer(IMapper mapper, CommsContext ctx, INotificationService service)
    {
        _mapper = mapper;
        _ctx = ctx;

        _service = service;
    }

    public async Task Consume(ConsumeContext<AddGeneralNotificationRq> context)
    {
        // Find existing notification
        var request = context.Message;
        var notification = await _ctx.Notifications.FirstOrDefaultAsync(x => x.UserLevel == request.UserLevel && x.Identifier == request.Identifier && x.Type == request.Type);
        if (notification != null)
        {
            return;
        }

        // Create new notification
        notification = new()
        {
            UserLevel = request.UserLevel,
            Identifier = request.Identifier,
            Type = request.Type,
            ContentRoute = request.Content.Route,
            ContentImageUrl = request.Content.ImageUrl,
            ReadAt = DateTime.UtcNow
        };
        notification = await _ctx.CreateAsync(notification);

        // Send update if not viewed
        await _service.TryToSend(notification, _mapper.Map<NotificationDto>(notification));
    }
}
