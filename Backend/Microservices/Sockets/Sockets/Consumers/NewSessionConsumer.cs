using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class NewSessionConsumer : ISnowConsumer, IConsumer<NewSessionRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public NewSessionConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<NewSessionRq> context)
    {
        await _hub.Clients.User($"{context.Message.UserId}").SendAsync("SessionAdded");
    }
}
