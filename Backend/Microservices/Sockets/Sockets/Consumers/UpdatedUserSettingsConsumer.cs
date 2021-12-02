using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class UpdatedUserSettingsConsumer: ISnowConsumer, IConsumer<UpdatedUserSettingsRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public UpdatedUserSettingsConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<UpdatedUserSettingsRq> context)
    {
        await _hub.Clients.User($"{ context.Message.UserId }").SendAsync("UpdatedUserSettings", context.Message.Settings);
    }
}
