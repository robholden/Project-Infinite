using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class UpdateUserFieldsConsumer : ISnowConsumer, IConsumer<UpdatedUserFieldsRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public UpdateUserFieldsConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<UpdatedUserFieldsRq> context)
    {
        await _hub.Clients.User($"{context.Message.UserId}").SendAsync("UpdatedUserFields", context.Message.NewValues);
    }
}
