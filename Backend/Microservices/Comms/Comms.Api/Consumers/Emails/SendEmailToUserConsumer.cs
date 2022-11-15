using Comms.Core.Services;
using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Emails;

public class SendEmailToUserConsumer : ISnowConsumer, IConsumer<SendEmailToUserRq>
{
    private readonly CommsContext _ctx;
    private readonly IEmailService _service;

    public SendEmailToUserConsumer(CommsContext ctx, IEmailService service)
    {
        _ctx = ctx;
        _service = service;
    }

    public async Task Consume(ConsumeContext<SendEmailToUserRq> context)
    {
        // In rare cases, we may not have a user yet. In this case, create one
        var request = context.Message;
        var user = await _ctx.UserSettings.FindOrNullAsync(x => x.UserId == request.User.UserId);

        // Create settings, if not found
        user ??= await _ctx.CreateAsync(new UserSetting(request.User.UserId));

        // Build email queue model with provided data
        var queue = new EmailQueue
        {
            Message = request.Message,
            Subject = request.Subject,
            UserId = request.User.UserId,
            Username = request.User.Username,
            IdentityHash = string.IsNullOrEmpty(request.IdentityHash) ? request.Subject : request.IdentityHash,
            Type = request.Type
        };
        await _service.Add(queue);
    }
}
