using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Users;

public class UpdatedNameConsumer: ISnowConsumer, IConsumer<UpdatedNameRq>
{
    private readonly CommsContext _ctx;

    public UpdatedNameConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UpdatedNameRq> context)
    {
        var request = context.Message;
        var user = await _ctx.UserSettings.FindOrNullAsync(x => x.UserId == request.UserId);
        if (user != null)
        {
            user.Name = request.Name;
            await _ctx.Put(user);
        }
    }
}
