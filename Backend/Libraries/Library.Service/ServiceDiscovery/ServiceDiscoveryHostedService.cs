
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
        if (string.IsNullOrEmpty(_config?.Name))
        {
            throw new ArgumentNullException(nameof(ServiceConfig));
        }

        while (string.IsNullOrEmpty(_serviceId))
        {
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

                _serviceId = registration.ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not register service with Consul => { ex.Message }");
                await Task.Delay(5000, cancellationToken);
            }
        }
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