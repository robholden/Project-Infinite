
using Library.Service.Api;
using Library.Service.ServiceDiscovery;

using Reports.Core;
using Reports.Core.Queries;
using Reports.Core.Services;
using Reports.Domain;

namespace Reports.Api;

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
        var dev = env.EnvironmentName == "Development";
        if (dev)
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.ConfigureStartup(true);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.RegisterMassTransit(Configuration);

        // Add auth
        services.RegisterAuth(Configuration);

        // Inject settings
        services.Configure<ReportSettings>(Configuration.GetSection("ReportSettings"));

        // Inject services
        services.AddTransient<IReportService, ReportService>();

        // Inject queries
        services.AddTransient<IReportQueries, ReportQueries>();

        // Add identity db
        services.AddDatabase<ReportContext>(Configuration, typeof(Startup).Assembly.GetName().Name);

        // Register this service for discovery
        services.DiscoverService(Configuration);
    }
}