using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class RevokeSessionConsumer : ISnowConsumer, IConsumer<RevokeSessionRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public RevokeSessionConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<RevokeSessionRq> context)
    {
        await _hub.Clients.User($"{context.Message.UserId}").SendAsync("SessionRevoked", context.Message.AuthToken);
    }
}
