using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Reports.Domain;

namespace Reports.Api.Consumers.Users;

public class UpdatedUsernameConsumer : ISnowConsumer, IConsumer<UpdatedUsernameRq>
{
    private readonly ReportContext _ctx;

    public UpdatedUsernameConsumer(ReportContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<UpdatedUsernameRq> context)
    {
        var request = context.Message;

        using var transaction = await _ctx.Database.BeginTransactionAsync();

        await _ctx.UpdateUsernameAsync<UserReport>(request.UserId, request.Username);
        await _ctx.UpdateUsernameAsync<ReportAction>(request.UserId, request.Username);

        await transaction.CommitAsync();
    }
}
