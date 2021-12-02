
using Comms.Domain;

namespace Comms.Core.Services;

public interface IEmailService
{
    Task Send(IEnumerable<EmailQueue> queues);

    Task Send(EmailQueue queue);

    Task DeleteQueues(IEnumerable<Guid> queueIds);

    Task<EmailQueue> Add(EmailQueue model);
}