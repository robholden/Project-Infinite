
using Comms.Core.Services;
using Comms.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Comms.Core.BackgroundTasks;

public class SendEmailsTask : BackgroundTask<SendEmailsTask>
{
    private static readonly Guid _ownerId = Guid.NewGuid();
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _running;

    public SendEmailsTask(ILogger<SendEmailsTask> logger, IServiceScopeFactory scopeFactory) : base(logger, 15)
    {
        _scopeFactory = scopeFactory;
    }

    public override async void DoWork(object state)
    {
        if (_running)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        // Get database context
        var ctx = scope.ServiceProvider.GetRequiredService<CommsContext>();

        _running = true;
        try
        {
            // Retrieve 50 emails from queue
            var queues = await ctx.EmailQueue
                .Include(x => x.Email)
                .Where(x =>
                    x.Sendable && !string.IsNullOrEmpty(x.EmailAddress)
                    && !x.Completed
                    && !x.OwnedBy.HasValue
                    && (x.Email != null && !x.Email.DateSent.HasValue)
                )
                .OrderBy(x => x.Date)
                .Take(50)
                .ToListAsync();

            // Stop if we have no emails
            if (queues.Count == 0)
            {
                return;
            }

            // Assign this batch an owner id

            queues.ForEach(x => x.OwnedBy = _ownerId);
            try
            {
                await ctx.UpdateManyAsync(queues);
                Console.WriteLine($"{_ownerId} now owns {queues.Count} emails");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_ownerId} failed to own emails: {ex.Message}");
                return;
            }

            // Send emails
            await scope.ServiceProvider.GetRequiredService<IEmailService>().Send(queues);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send queues");
        }
        finally
        {
            try
            {
                // Update queues
                var myQueues = ctx.EmailQueue
                    .Include(x => x.Email)
                    .Where(x => x.OwnedBy == _ownerId);

                await myQueues.ForEachAsync(queue =>
                {
                    queue.OwnedBy = null;

                    if (queue.Email?.DateSent.HasValue == true)
                    {
                        queue.Completed = true;
                        queue.CompletedAt = queue.Email.DateSent.Value;
                    }
                    else if (queue.Email?.Attempts >= 3)
                    {
                        queue.Completed = true;
                    }
                });

                await ctx.UpdateManyAsync(myQueues);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed update email queue after sending");
            }

            _running = false;
        }
    }
}