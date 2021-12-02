using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class InvalidateUsersConsumer: ISnowConsumer, IConsumer<InvalidateUsersRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public InvalidateUsersConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<InvalidateUsersRq> context)
    {
        await _hub.Clients.All.SendAsync("InvalidateUser");
    }
}
