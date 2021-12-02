
using Consul;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Library.Service.ServiceDiscovery;

public static class ServiceDiscoveryExtensions
{
    public static void DiscoverService(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceConfig = new ServiceConfig
        {
            DiscoveryAddress = configuration.GetValue<Uri>("ServiceConfig:DiscoveryAddress"),
            Address = configuration.GetValue<Uri>("ServiceConfig:Address"),
            Name = configuration.GetValue<string>("ServiceConfig:Name")
        };

        if (serviceConfig.DiscoveryAddress == null)
        {
            return;
        }

        services.AddSingleton(serviceConfig);
        services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
        services.AddSingleton<IConsulClient, ConsulClient>(_ => new ConsulClient(cfg => cfg.Address = serviceConfig.DiscoveryAddress));
    }
}