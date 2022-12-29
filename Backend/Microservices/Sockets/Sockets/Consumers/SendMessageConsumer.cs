using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Consumers;

public class SendMessageConsumer : IRabbitConsumer, IConsumer<SendMessageRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public SendMessageConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<SendMessageRq> context)
    {
        var req = context.Message;
        IClientProxy client;

        if (req.UserId.HasValue)
        {
            client = _hub.Clients.User($"{req.UserId}");
        }
        else if (req.UserId.HasValue)
        {
            client = _hub.Clients.Group($"{req.UserLevel}");
        }
        else
        {
            client = _hub.Clients.All;
        }

        if (req.Data == null) await client.SendAsync(req.Method);
        else await client.SendAsync(req.Method, req.Data);
    }
}
