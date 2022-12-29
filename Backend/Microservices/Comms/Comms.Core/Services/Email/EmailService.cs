using System.Net;
using System.Net.Mail;

using Comms.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Comms.Core.Services;

public class EmailService : IEmailService
{
    private readonly CommsContext _ctx;
    private readonly EmailSettings _settings;

    public EmailService(CommsContext ctx, IOptions<EmailSettings> settings)
    {
        _ctx = ctx;
        _settings = settings.Value;
    }

    public Task Send(EmailQueue queue) => Send(new List<EmailQueue>() { queue });

    public async Task Send(Guid queueId)
    {
        var queue = await _ctx.EmailQueue.FindAsync(e => e.EmailQueueId == queueId && !e.Completed);
        if (queue != null) await Send(queue);
    }

    public async Task Send(IEnumerable<EmailQueue> queues)
    {
        // Find existing email entry or create one
        var emails = queues.Where(x => x.Email != null).Select(x => x.Email).ToList();
        var newEmails = queues.Where(x => x.Email == null).Select(queue =>
        {
            // Set opting out text
            var optOutText = string.Empty;
            if (queue.OptOutKey != null)
            {
                optOutText = $"<p>If you no longer wish to receive these emails, click to <a href='###SITE_URL###/unsubscribe/{queue.OptOutKey}'>unsubscribe</a>.</p>";
            }

            // Create html body text
            var body = $@"
                    <div style='font-family: Arial, Helvetica, sans-serif;'
                        <p>
                            Hello {queue.Name},
                        </p>
                        <p>
                            {queue.Message}
                        </p>
                        <p>
                            Regards,
                            <br />
                            {_settings.Name}
                        </p>
                        {optOutText}
                    </div>
                ";

            // Replace body keys
            body = body
            .Replace("###SITE_NAME###", _settings.Name)
            .Replace("###SITE_URL###", _settings.WebUrl);

            return new Email
            {
                Body = body,
                FromEmailAddress = _settings.EmailAddress,
                EmailQueueId = queue.EmailQueueId,
                EmailQueue = queue
            };
        }).ToList();

        // Create entries into database
        emails.AddRange(await _ctx.CreateManyAsync(newEmails));

        var error = string.Empty;
        try
        {
            // Connect to smtp server
            using var client = new SmtpClient
            {
                Host = _settings.Host,
                Port = _settings.Port,
                EnableSsl = _settings.UseSSL,
                UseDefaultCredentials = _settings.UseCredentials
            };

            // Authenticate with server
            if (_settings.UseCredentials)
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            // Loop through each email and send
            foreach (var email in emails)
            {
                try
                {
                    // Create new mail message with our settings
                    using var mm = new MailMessage { From = new MailAddress(_settings.EmailAddress, _settings.Name) };

                    mm.IsBodyHtml = true;
                    mm.Subject = email.EmailQueue.Subject;
                    mm.Body = email.Body;

                    // Verify email we are sending too
                    if (!MailAddress.TryCreate(email.EmailQueue.EmailAddress, out var toEmail))
                    {
                        email.Errors = "Invalid email address";
                    }

                    // Send email :)
                    else
                    {
                        mm.To.Add(toEmail);
                        await client.SendMailAsync(mm);

                        email.DateSent = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    email.Errors = $"Sending Error: {ex.Message}";
                }
                finally
                {
                    email.Completed = true;
                    email.Attempts++;
                }
            }
        }
        catch (Exception ex)
        {
            emails.ForEach(email =>
            {
                email.Errors = $"Connection Error: {ex.Message}";
                email.Completed = true;
                email.Attempts++;
            });
        }

        await _ctx.UpdateManyAsync(emails);
    }

    public async Task DeleteQueues(IEnumerable<Guid> queueIds)
    {
        await _ctx.ExecuteDeleteAsync<Email>(e => queueIds.Contains(e.EmailQueueId));
    }

    public async Task<EmailQueue> CreateAndSend(EmailQueue model)
    {
        var email = await Queue(model);
        await Send(email);

        return email;
    }

    public async Task<EmailQueue> Queue(EmailQueue model)
    {
        // Stop if we have sent already
        if (!await CanSend(model))
        {
            return null;
        }

        // Build email queue model
        return await _ctx.CreateAsync(model);
    }

    private async Task<bool> CanSend(EmailQueue queue)
    {
        // Find other emails with this email & hash
        var existing = _ctx.EmailQueue.Where(x => x.EmailAddress == queue.EmailAddress && x.IdentityHash == queue.IdentityHash);
        if (!await existing.AnyAsync())
        {
            return true;
        }

        // Remove older entries
        _ctx.EmailQueue.RemoveRange(existing.Where(x => x.Completed && !x.CompletedAt.HasValue));
        await _ctx.SaveChangesAsync();

        // Check for recently sent email
        var threshold = DateTime.UtcNow.AddSeconds(-30);
        if (await existing.AnyAsync(x => x.CompletedAt > threshold))
        {
            return false;
        }

        return true;
    }
}