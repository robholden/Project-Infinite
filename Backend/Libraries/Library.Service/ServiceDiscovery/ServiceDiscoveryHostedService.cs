
using Consul;

using Microsoft.Extensions.Hosting;

namespace Library.Service.ServiceDiscovery;

public class ServiceDiscoveryHostedService : IHostedService
{
    private readonly IConsulClient _client;
    private readonly ServiceConfig _config;

    private string _serviceId;

    public ServiceDiscoveryHostedService(IConsulClient client, ServiceConfig config)
    {
        _client = client;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_config == null)
        {
            return;
        }

        var registration = new AgentServiceRegistration
        {
            ID = _config.Name,
            Name = _config.Name,
            Address = _config.Address.Host,
            Port = _config.Address.Port
        };

        try
        {
            await _client.Agent.ServiceDeregister(registration.ID, cancellationToken);
            await _client.Agent.ServiceRegister(registration, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not register service with Consul => { ex.Message }");
        }

        _serviceId = registration.ID;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceId == null)
        {
            return;
        }

        await _client.Agent.ServiceDeregister(_serviceId, cancellationToken);
    }
}