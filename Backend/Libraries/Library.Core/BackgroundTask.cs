
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Library.Core;

public interface IBackgroundTask
{
    void Dispose();

    void DoWork(object state);
}

public abstract class BackgroundTask<T> : IHostedService, IBackgroundTask, IDisposable
{
    protected readonly ILogger<T> logger;
    private readonly int _interval;
    private Timer _timer;

    protected BackgroundTask(ILogger<T> logger, int interval)
    {
        this.logger = logger;
        _interval = interval;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(T)} Service running.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(_interval));

        return Task.CompletedTask;
    }

    public abstract void DoWork(object state);

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(T)} Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}