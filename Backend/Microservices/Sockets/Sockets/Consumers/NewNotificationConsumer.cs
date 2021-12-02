using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class NewNotificationConsumer : ISnowConsumer, IConsumer<NewNotificationRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public NewNotificationConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<NewNotificationRq> context)
    {
        var request = context.Message;

        if (request.UserId != new Guid())
        {
            await _hub.Clients.User($"{ request.UserId }").SendAsync("NewNotification", request.Notification);
        }
        else if (request.UserLevel.HasValue)
        {
            await _hub.Clients.Groups(request.UserLevel.ToString()).SendAsync($"{ request.UserLevel }NewNotification", request.Notification);
        }
    }
}
