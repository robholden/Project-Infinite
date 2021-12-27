
using Library.Service.Api;

using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway;

public class Startup
{
    private readonly bool _useProxy;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        _useProxy = configuration.GetValue<bool>("ProxyEnabled");
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.AddCors();

        if (_useProxy) services.AddOcelot();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevMode())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseCors(x => x
            .BuildOrigins(Configuration["AllowedOrigins"])
            .AllowAnyMethod()
            .AllowAnyHeader()
        );

        app.ConfigureStartup(false);

        // Start ocelot
        if (_useProxy)
        {
            app.UseWebSockets();
            await app.UseOcelot();
        }
    }
}
