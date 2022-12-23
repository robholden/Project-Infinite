using Comms.Core.Services;
using Comms.Domain;

using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Emails;

public class SendEmailToUserConsumer : ISnowConsumer, IConsumer<SendEmailToUserRq>
{
    private readonly IEmailService _service;

    public SendEmailToUserConsumer(IEmailService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<SendEmailToUserRq> context)
    {
        // Build email queue model with provided data
        var queue = EmailQueue.FromUserReq(context.Message);
        var email = await _service.Queue(queue);
        if (queue.Sendable)
        {
            await _service.Send(email);
        }
    }
}
