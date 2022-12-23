
using Library.Core;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public record ReportUserRq(UserRecord User, ReportUserReason Reason, UserRecord ReportedUser, string Name, string Email);

public interface IReportPubSub
{
    Task ReportUser(ReportUserRq payload);
}

public class ReportPubSub : BasePubSub, IReportPubSub
{
    public ReportPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task ReportUser(ReportUserRq payload) => await Publish(payload);

    public static void AddRequestClients(IBusRegistrationConfigurator configurator)
    {
    }
}