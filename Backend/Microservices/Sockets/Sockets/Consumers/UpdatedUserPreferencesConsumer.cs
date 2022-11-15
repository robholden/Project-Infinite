using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class UpdatedUserPreferencesConsumer : ISnowConsumer, IConsumer<UpdatedUserPreferencesRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public UpdatedUserPreferencesConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<UpdatedUserPreferencesRq> context)
    {
        await _hub.Clients.User($"{context.Message.UserId}").SendAsync("UpdatedUserPreferences", context.Message.Preferences);
    }
}
