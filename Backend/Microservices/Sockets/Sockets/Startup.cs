using Library.Service.Api;

namespace Sockets;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UsePathBase("/sockets");
        app.ConfigureStartup(Configuration, env, true, endpoints =>
        {
            endpoints.MapHub<SocketHub>("/hubs/hub");
        });
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.RegisterServices(Configuration);
        services.RegisterMassTransit("sockets", Configuration);

        // Enable cors (required for SignalR)
        services.AddCors();

        // Add auth
        services.RegisterAuth(Configuration);

        // Setup SignalR 
        services.AddSignalR()
            .AddStackExchangeRedis(Configuration.GetConnectionString("Redis"), options => options.Configuration.ChannelPrefix = "SnowCapture");
    }
}