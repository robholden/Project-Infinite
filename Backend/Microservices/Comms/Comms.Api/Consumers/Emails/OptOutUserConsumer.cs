using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Emails;

public class OptOutUserConsumer: ISnowConsumer, IConsumer<OptOutUserRq>
{
    private readonly CommsContext _ctx;

    public OptOutUserConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<OptOutUserRq> context)
    {
        var user = await _ctx.UserSettings.FirstOrDefaultAsync(x => x.UserId == context.Message.UserId);
        if (user?.MarketingEmail != true) return;

        user.MarketingEmail = false;
        user.MarketingOptOutKey = null;

        await _ctx.Put(user);
    }
}
