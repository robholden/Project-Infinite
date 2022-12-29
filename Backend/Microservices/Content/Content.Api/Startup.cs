using Content.Core;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;

using Library.Service.Api;

using Microsoft.Extensions.Caching.Memory;

namespace Content.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCache cache)
    {
        app.UsePathBase("/content");
        app.UseStaticFiles();
        app.ConfigureStartup(Configuration, env, true);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.RegisterMassTransit("content", Configuration);

        // Add auth
        services.RegisterAuth(Configuration);

        // Inject settings
        services.Configure<ContentSettings>(Configuration.GetSection("ContentSettings"));

        // Inject services
        services.AddTransient<IPostService, PostService>();

        // Inject queries
        services.AddTransient<IPostQueries, PostQueries>();

        // Add identity db
        services.AddDatabase<ContentContext>(Configuration, typeof(Startup).Assembly.GetName().Name);
    }
}