using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Emails;

public class UpdateEmailTypeConsumer : ISnowConsumer, IConsumer<UpdateEmailTypeRq>
{
    private readonly CommsContext _ctx;

    public UpdateEmailTypeConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UpdateEmailTypeRq> context)
    {
        // Get email from identity hash
        var queueQuery = _ctx.EmailQueue.Where(x => x.IdentityHash == context.Message.IdentityHash);
        if (!await queueQuery.AnyAsync())
        {
            return;
        }

        // Convert query to list
        var queues = await queueQuery.ToListAsync();

        // Update email type for each entry
        queues.ForEach(x => x.Type = context.Message.Type);

        // Update db
        await _ctx.UpdateAsync(queues);
    }
}
