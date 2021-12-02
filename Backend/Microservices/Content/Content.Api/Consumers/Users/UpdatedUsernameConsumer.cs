using Content.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Content.Api.Consumers.Users;

public class UpdatedUsernameConsumer : ISnowConsumer, IConsumer<UpdatedUsernameRq>
{
    private readonly ContentContext _ctx;

    public UpdatedUsernameConsumer(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UpdatedUsernameRq> context)
    {
        var request = context.Message;

        using var transaction = await _ctx.Database.BeginTransactionAsync();

        await _ctx.UpdateUsernameAsync<Picture>(request.UserId, request.Username);
        await _ctx.UpdateUsernameAsync<PictureLike>(request.UserId, request.Username);
        await _ctx.UpdateUsernameAsync<Collection>(request.UserId, request.Username);

        await transaction.CommitAsync();
    }
}
