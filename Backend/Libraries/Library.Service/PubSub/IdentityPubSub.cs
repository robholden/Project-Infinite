using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record UpdatedUsernameRq(Guid UserId, string Username);

public record UpdatedEmailRq(Guid UserId, string Email);

public record UpdatedNameRq(Guid UserId, string Name);

public record DeletedUserRq(Guid UserId);

public record DeleteReportedUserRq(Guid UserId, bool SendEmail);

public interface IIdentityPubSub
{
    Task UpdatedUsername(UpdatedUsernameRq payload);

    Task UpdatedEmail(UpdatedEmailRq payload);

    Task UpdatedName(UpdatedNameRq payload);

    Task DeletedUser(DeletedUserRq payload);

    Task DeleteReportedUser(DeleteReportedUserRq payload);
}

public class IdentityPubSub : BasePubSub, IIdentityPubSub
{
    public IdentityPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task UpdatedUsername(UpdatedUsernameRq payload) => await Publish(payload);

    public async Task UpdatedEmail(UpdatedEmailRq payload) => await Publish(payload);

    public async Task UpdatedName(UpdatedNameRq payload) => await Publish(payload);

    public async Task DeletedUser(DeletedUserRq payload) => await Publish(payload);

    public async Task DeleteReportedUser(DeleteReportedUserRq payload) => await Publish(payload);

    public static void AddRequestClients(IServiceCollectionBusConfigurator configurator)
    {
    }
}