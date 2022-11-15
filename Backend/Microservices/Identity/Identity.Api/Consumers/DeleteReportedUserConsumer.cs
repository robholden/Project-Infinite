
using Identity.Core.Services;

using Library.Service.PubSub;

using MassTransit;

namespace Identity.Api.Consumers;

public class DeleteReportedUserConsumer : ISnowConsumer, IConsumer<DeleteReportedUserRq>
{
    private readonly IUserService _service;
    private readonly ICommsPubSub _commEvents;

    public DeleteReportedUserConsumer(IUserService service, ICommsPubSub commEvents)
    {
        _service = service;
        _commEvents = commEvents;
    }

    public async Task Consume(ConsumeContext<DeleteReportedUserRq> context)
    {
        // Delete the user's account
        var request = context.Message;
        var user = await _service.DeleteAccount(request.UserId);
        if (user == null)
        {
            return;
        }

        // Send email to user
        if (!request.SendEmail)
        {
            return;
        }

        var subject = "We've removed your account";
        var message = "We're informing you your account has violated our site policies. Due to this, we have permanently deleted your account from our site.";

        _ = _commEvents?.SendEmailDirectly(new(user.Name, user.Email, message, subject));
    }
}
