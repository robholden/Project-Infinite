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

    public async Task Send(IEnumerable<EmailQueue> queues)
    {
        // Find existing email entry or create one
        var emails = queues.Where(x => x.Email != null).Select(x => x.Email).ToList();
        var newEmails = queues.Where(x => x.Email == null).Select(queue =>
        {
            if (queue.Email != null)
            {
                return queue.Email;
            }

            // Set opting out text
            var optOutText = string.Empty;
            if (queue.User?.MarketingOptOutKey != null)
            {
                optOutText = $"<p>If you no longer wish to receive these emails, click to <a href='###SITE_URL###/unsubscribe/{queue.User.MarketingOptOutKey}'>unsubscribe</a>.</p>";
            }

            // Create html body text
            // TODO: Use template
            var body = $@"
                    <div style='font-family: Arial, Helvetica, sans-serif;'
                        <p>
                            Hello {queue.User?.Name ?? queue.Name},
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
        });

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
                    if (!MailAddress.TryCreate(email.EmailQueue.User?.Email ?? email.EmailQueue.EmailAddress, out var toEmail))
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
        await _ctx.Emails.Where(e => queueIds.Contains(e.EmailQueueId)).ExecuteDeleteAsync();
    }

    public async Task<EmailQueue> Add(EmailQueue model)
    {
        // Stop if we have sent already
        if (!await CanSend(model))
        {
            return null;
        }

        // Build email queue model
        var queue = await _ctx.CreateAsync(model);

        // Send email when typeof instant
        if (model.Type == EmailType.Instant)
        {
            await Send(queue);
        }

        return queue;
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
        await existing.Where(x => x.Completed && !x.CompletedAt.HasValue).ExecuteDeleteAsync();

        // Check for recently sent email
        var threshold = queue.Type switch
        {
            EmailType.Instant => DateTime.UtcNow.AddSeconds(-30),
            EmailType.System => DateTime.UtcNow.AddMinutes(-1),
            _ => DateTime.UtcNow.AddHours(-12),
        };
        if (await existing.AnyAsync(x => x.CompletedAt > threshold))
        {
            return false;
        }

        return true;
    }
}