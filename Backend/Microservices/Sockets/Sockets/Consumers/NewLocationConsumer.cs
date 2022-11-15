using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class NewLocationConsumer : ISnowConsumer, IConsumer<NewLocationRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public NewLocationConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<NewLocationRq> context)
    {
        var request = context.Message;
        var location = new
        {
            request.Name,
            Country = new { Name = request.Country },
            request.Lat,
            request.Lng
        };
        await _hub.Clients.User($"{request.UserId}").SendAsync("NewPictureLocation", new { pictureId = request.PictureId, location });
    }
}
