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
        // JSON doesn't convert keys in dictionaries
        //  e.g. { EmailConfirmed = true } translates to { "EmailedConfirmed": true }
        // 
        // Ensure all properties have their first letter lowercased
        var changes = context.Message.NewValues.ToDictionary(
            c => char.IsUpper(c.Key[0]) ? c.Key.Length == 1 ? char.ToLower(c.Key[0]).ToString() : char.ToLower(c.Key[0]) + c.Key[1..] : c.Key,
            c => c.Value
        );

        await _hub.Clients.User($"{context.Message.UserId}").SendAsync("UpdatedUserFields", changes);
    }
}
