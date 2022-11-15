
using Content.Core.Services;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

namespace Content.Api.Consumers.Pictures;

public class DeleteReportedPictureConsumer : ISnowConsumer, IConsumer<DeleteReportedPictureRq>
{
    private readonly IPictureService _service;
    private readonly ICommsPubSub _commEvents;

    public DeleteReportedPictureConsumer(IPictureService service, ICommsPubSub commEvents)
    {
        _service = service;
        _commEvents = commEvents;
    }

    public async Task Consume(ConsumeContext<DeleteReportedPictureRq> context)
    {
        // Delete the user's picture
        var request = context.Message;
        var picture = await _service.Delete(request.PictureId);

        // Send email to user
        if (request.SendEmail)
        {
            var subject = "We've removed your picture";
            var message = $"We're informing you your picture \"{picture.Name}\" has violated our site policies. Due to this, we have permanently deleted the picture from our site.";

            _ = _commEvents?.SendEmailToUser(new(picture.ToUserRecord(), message, subject, EmailType.Instant));
        }
    }
}
