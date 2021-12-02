using Content.Core.Services;
using Content.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Content.Api.Consumers.Users;

public class DeletedUserConsumer : ISnowConsumer, IConsumer<DeletedUserRq>
{
    private readonly ContentContext _ctx;
    private readonly IPictureService _pictureService;

    public DeletedUserConsumer(ContentContext ctx, IPictureService pictureService)
    {
        _ctx = ctx;
        _pictureService = pictureService;
    }

    public async Task Consume(ConsumeContext<DeletedUserRq> context)
    {
        var request = context.Message;

        using var transaction = await _ctx.Database.BeginTransactionAsync();

        await _pictureService.DeleteByUser(request.UserId);
        await _ctx.DeleteUserAsync<PictureLike>(request.UserId);
        await _ctx.DeleteUserAsync<Collection>(request.UserId);
        await _ctx.DeleteUserAsync<UserSetting>(request.UserId);

        await transaction.CommitAsync();
    }
}
