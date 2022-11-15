using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Users;

public class UpdatedEmailConsumer : ISnowConsumer, IConsumer<UpdatedEmailRq>
{
    private readonly CommsContext _ctx;

    public UpdatedEmailConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UpdatedEmailRq> context)
    {
        var request = context.Message;
        var user = await _ctx.UserSettings.FindOrNullAsync(x => x.UserId == request.UserId);
        if (user != null)
        {
            user.Email = request.Email;
            await _ctx.UpdateAsync(user);
        }
    }
}
