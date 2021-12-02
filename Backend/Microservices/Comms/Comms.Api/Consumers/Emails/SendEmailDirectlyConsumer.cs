using Comms.Core.Services;
using Comms.Domain;

using Library.Service.PubSub;

using MassTransit;

namespace Comms.Api.Consumers.Emails;

public class SendEmailDirectlyConsumer: ISnowConsumer, IConsumer<SendEmailDirectlyRq>
{
    private readonly IEmailService _service;

    public SendEmailDirectlyConsumer(IEmailService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<SendEmailDirectlyRq> context)
    {
        // Build email queue model with provided data
        var request = context.Message;
        var queue = new EmailQueue
        {
            Message = request.Message,
            Subject = request.Subject,
            EmailAddress = request.Email,
            Name = request.Name,
            IdentityHash = string.IsNullOrEmpty(request.IdentityHash) ? request.Subject : request.IdentityHash
        };
        queue = await _service.Add(queue);

        // Send email immediately
        if (queue != null)
        {
            await _service.Send(queue);
        }
    }
}
