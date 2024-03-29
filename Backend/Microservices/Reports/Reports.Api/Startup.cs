﻿
using Library.Service.Api;

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
        app.UsePathBase("/reports");
        app.ConfigureStartup(Configuration, env, true);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.RegisterMassTransit("reports", Configuration);

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
    }
}