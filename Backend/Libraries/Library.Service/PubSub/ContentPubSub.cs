
using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public interface IContentPubSub
{
}

public class ContentPubSub : BasePubSub, IContentPubSub
{
    public ContentPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
    }
}