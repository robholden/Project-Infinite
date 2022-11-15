using AutoMapper;

using Comms.Api.Dtos;
using Comms.Core.Services;
using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Notifications;

public class AddNotificationConsumer : ISnowConsumer, IConsumer<AddNotificationRq>
{
    private readonly IMapper _mapper;
    private readonly CommsContext _ctx;

    private readonly INotificationService _service;

    public AddNotificationConsumer(IMapper mapper, CommsContext ctx, INotificationService service)
    {
        _mapper = mapper;
        _ctx = ctx;

        _service = service;
    }

    public async Task Consume(ConsumeContext<AddNotificationRq> context)
    {
        // Ignore self notifications
        var request = context.Message;
        if (request.User.UserId == request.TriggeredUser?.UserId)
        {
            return;
        }

        // Find existing notification
        var notification = await _ctx.Notifications
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.UserId == request.User.UserId && x.Type == request.Type && x.ContentKey == request.Content.Key);

        // Create notification if there isn't one
        if (notification == null)
        {
            notification = new()
            {
                UserId = request.User.UserId,
                Username = request.User.Username,
                ContentKey = request.Content.Key,
                ContentMessage = request.Content.Message,
                ContentImage = request.Content.Image,
                Type = request.Type,
                Entries = new List<NotificationEntry>()
            };

            if (request.TriggeredUser != null)
            {
                notification.Entries.Add(new(request.TriggeredUser));
            }

            notification = await _ctx.CreateAsync(notification);
        }

        // Update current notification
        else
        {
            notification.ViewedAt = null;
            notification.ReadAt = null;
            notification.Date = DateTime.UtcNow;
            notification.Hidden = false;

            // Add or update user entry
            if (request.TriggeredUser != null)
            {
                // Find existing entry and undelete
                var entry = notification.Entries.FirstOrDefault(x => x.NotificationId == notification.NotificationId && x.UserId == request.TriggeredUser.UserId);
                if (entry != null)
                {
                    entry.Deleted = !entry.Deleted;
                }

                // Create new entry
                else
                {
                    entry = new(notification.NotificationId, request.TriggeredUser);
                    notification.Entries.Add(entry);
                }
            }

            notification = await _ctx.UpdateAsync(notification);
        }

        // Send update
        await _service.TryToSend(notification, _mapper.Map<NotificationDto>(notification));
    }
}
