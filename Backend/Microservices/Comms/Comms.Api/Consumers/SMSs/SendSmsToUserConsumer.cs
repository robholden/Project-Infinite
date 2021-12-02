using Comms.Core.SMS;
using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Comms.Api.Consumers.SMSs;

public class SendSmsToUserConsumer: ISnowConsumer, IConsumer<SendSmsToUserRq>
{
    private readonly CommsContext _ctx;
    private readonly ISmsProvider _smsProvider;

    public SendSmsToUserConsumer(CommsContext ctx, ISmsProvider smsProvider)
    {
        _ctx = ctx;
        _smsProvider = smsProvider;
    }

    public async Task Consume(ConsumeContext<SendSmsToUserRq> context)
    {
        // Check a message hasn't already been sent within the last 30 seconds
        var request = context.Message;
        var threshold = DateTime.UtcNow.AddSeconds(-30);
        if (await _ctx.Sms.AnyAsync(s => s.UserId == request.User.UserId && s.DateSent > threshold))
        {
            return;
        }

        // Get user info
        var sms = new Sms
        {
            Message = request.Message,
            Mobile = request.Mobile,
            UserId = request.User.UserId,
            Username = request.User.Username
        };

        // Call TextMagic api
        var (sent, error) = await _smsProvider.SendAsync(sms.Mobile, sms.Message);

        sms.DateSent = DateTime.UtcNow;
        sms.Sent = sent;
        await _ctx.Post(sms);

        // TODO: How to trigger error
        if (!sms.Sent)
        {
        }
    }
}
