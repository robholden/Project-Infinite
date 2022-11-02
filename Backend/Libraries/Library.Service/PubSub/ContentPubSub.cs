
using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record DeleteReportedPictureRq(Guid PictureId, bool SendEmail);

public interface IContentPubSub
{
    Task DeleteReportedPicture(DeleteReportedPictureRq payload);
}

public class ContentPubSub : BasePubSub, IContentPubSub
{
    public ContentPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task DeleteReportedPicture(DeleteReportedPictureRq payload) => await Publish(payload);

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
    }
}