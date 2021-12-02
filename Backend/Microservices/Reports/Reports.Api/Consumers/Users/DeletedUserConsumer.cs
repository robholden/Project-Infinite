
using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Reports.Domain;

namespace Reports.Api.Consumers.Users;

public class DeletedUserConsumer: ISnowConsumer, IConsumer<DeletedUserRq>
{
    private readonly ReportContext _ctx;

    public DeletedUserConsumer(ReportContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<DeletedUserRq> context)
    {
        var request = context.Message;

        using var transaction = await _ctx.Database.BeginTransactionAsync();

        await _ctx.DeleteUserAsync<UserReport>(request.UserId);
        await _ctx.DeleteUserAsync<PictureReport>(request.UserId);
        await _ctx.DeleteUserAsync<ReportAction>(request.UserId);

        await transaction.CommitAsync();
    }
}
