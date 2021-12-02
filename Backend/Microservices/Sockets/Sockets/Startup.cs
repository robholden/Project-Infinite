
using Library.Service.Api;
using Library.Service.ServiceDiscovery;

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
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(x => x
            .BuildOrigins(Configuration["AllowedOrigins"])
            .AllowAnyMethod()
            .AllowAnyHeader()
        );

        app.ConfigureStartup(true, endpoints => endpoints.MapHub<SocketHub>("/hubs/hub"));
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.RegisterServices(Configuration);
        services.RegisterMassTransit(Configuration);

        // Enable cors (required for SignalR)
        services.AddCors();

        // Add auth
        services.RegisterAuth(Configuration);

        services.AddSignalR()
            .AddNewtonsoftJsonProtocol()
            .AddStackExchangeRedis(Configuration.GetConnectionString("Redis"), options => options.Configuration.ChannelPrefix = "SnowCapture");

        // Register this service for discovery
        services.DiscoverService(Configuration);
    }
}