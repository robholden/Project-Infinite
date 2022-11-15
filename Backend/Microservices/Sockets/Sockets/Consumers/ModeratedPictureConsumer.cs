using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Sockets.Api.Consumers;

public class ModeratedPictureConsumer : ISnowConsumer, IConsumer<ModeratedPictureRq>
{
    private readonly IHubContext<SocketHub> _hub;

    public ModeratedPictureConsumer(IHubContext<SocketHub> hub)
    {
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<ModeratedPictureRq> context)
    {
        await _hub.Clients.Groups(nameof(UserLevel.Moderator)).SendAsync("ModeratedPicture", context.Message.PictureId, context.Message.Approved);
    }
}
