
using Comms.Domain;

namespace Comms.Core.Services;

public interface IEmailService
{
    Task Send(IEnumerable<EmailQueue> queues);

    Task Send(EmailQueue queue);

    Task Send(Guid queueId);

    Task DeleteQueues(IEnumerable<Guid> queueIds);

    Task<EmailQueue> CreateAndSend(EmailQueue model);

    Task<EmailQueue> Queue(EmailQueue model);
}