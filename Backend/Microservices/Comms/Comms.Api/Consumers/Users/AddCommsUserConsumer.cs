using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.Users;

public class AddCommsUserConsumer : ISnowConsumer, IConsumer<AddCommsUserRq>
{
    private readonly CommsContext _ctx;

    public AddCommsUserConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<AddCommsUserRq> context)
    {
        SnowConsumerResponse response;

        try
        {
            // Add user to the database
            var request = context.Message;
            var user = await _ctx.UserSettings.FirstOrDefaultAsync(x => x.UserId == request.User.UserId);
            var exists = user != null;

            var data = new UserSetting
            {
                UserId = request.User.UserId,
                Name = request.Name,
                Email = request.Email,
                MarketingEmail = request.Marketing,
                MarketingOptOutKey = request.Marketing ? Guid.NewGuid() : null
            };

            if (!exists) await _ctx.Post(data);
            else await _ctx.Put(data);

            response = new();
        }
        catch (Exception ex)
        {
            response = new(ex);
        }

        await context.RespondAsync(response);
    }
}
