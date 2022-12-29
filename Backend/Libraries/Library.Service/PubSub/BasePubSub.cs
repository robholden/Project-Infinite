
using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public abstract class BasePubSub
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected readonly IBus bus;

    protected BasePubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory)
    {
        bus = publishEndpoint;
        _scopeFactory = scopeFactory;
    }

    protected async Task Publish<T>(T payload) where T : class
    {
        await bus.Publish(payload);
    }

    protected async Task<RabbitConsumerResponse> PublishWithResult<T>(T payload) where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<IRequestClient<T>>();
        var response = await client.GetResponse<RabbitConsumerResponse>(payload);

        if (response?.Message?.Error != null)
        {
            response.Message.Error.Throw();
        }

        return response?.Message;
    }

    protected async Task<RabbitConsumerResponse<O>> PublishWithResult<T, O>(T payload) where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<IRequestClient<T>>();
        var response = await client.GetResponse<RabbitConsumerResponse<O>>(payload);

        if (response?.Message?.Error != null)
        {
            response.Message.Error.Throw();
        }

        return response?.Message;
    }
}

public static class PubSubExtensions
{
    public static void AddPubSubs(this IServiceCollection services, IBusRegistrationConfigurator configurator)
    {
        CommsPubSub.AddRequestClients(configurator);
        ContentPubSub.AddRequestClients(configurator);
        IdentityPubSub.AddRequestClients(configurator);
        ReportPubSub.AddRequestClients(configurator);

        services.AddTransient<ICommsPubSub, CommsPubSub>();
        services.AddTransient<IContentPubSub, ContentPubSub>();
        services.AddTransient<IIdentityPubSub, IdentityPubSub>();
        services.AddTransient<ISocketsPubSub, SocketsPubSub>();
        services.AddTransient<IReportPubSub, ReportPubSub>();
    }
}