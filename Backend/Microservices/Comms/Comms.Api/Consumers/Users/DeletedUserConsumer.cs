using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Users;

public class DeletedUserConsumer: ISnowConsumer, IConsumer<DeletedUserRq>
{
    private readonly CommsContext _ctx;

    public DeletedUserConsumer(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<DeletedUserRq> context)
    {
        var request = context.Message;

        using var transaction = await _ctx.Database.BeginTransactionAsync();

        await _ctx.DeleteUserAsync<EmailQueue>(request.UserId);
        await _ctx.DeleteUserAsync<Notification>(request.UserId);
        await _ctx.DeleteUserAsync<NotificationEntry>(request.UserId);
        await _ctx.DeleteUserAsync<Sms>(request.UserId);
        await _ctx.DeleteUserAsync<UserSetting>(request.UserId);

        await transaction.CommitAsync();
    }
}
