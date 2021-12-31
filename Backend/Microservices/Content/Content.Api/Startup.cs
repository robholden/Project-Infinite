using Content.Api.Templates;
using Content.Core;
using Content.Core.BackgroundTasks;
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
        app.UseStaticFiles();

        app.ConfigureStartup(Configuration, env, true);

        _ = Task.Run(async () => await cache.CacheTemplates());
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
        services.Configure<PictureSettings>(Configuration.GetSection("PictureSettings"));

        // Inject services
        services.AddTransient<IUserSettingService, UserSettingService>();
        services.AddTransient<ILocationService, LocationService>();
        services.AddTransient<IPictureService, PictureService>();
        services.AddTransient<ICollectionService, CollectionService>();
        services.AddTransient<ITagService, TagService>();

        // Inject queries
        services.AddTransient<IUserSettingQueries, UserSettingQueries>();
        services.AddTransient<ILocationQueries, LocationQueries>();
        services.AddTransient<IPictureQueries, PictureQueries>();
        services.AddTransient<IPictureModerationQueries, PictureModerationQueries>();
        services.AddTransient<ICollectionQueries, CollectionQueries>();
        services.AddTransient<ITagQueries, TagQueries>();

        // Add identity db
        services.AddDatabase<ContentContext>(Configuration, typeof(Startup).Assembly.GetName().Name);

        // Register background tasks
        services.AddHostedService<CreateLocationsTask>();
    }
}