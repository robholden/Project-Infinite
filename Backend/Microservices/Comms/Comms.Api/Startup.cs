﻿
using Comms.Core;
using Comms.Core.BackgroundTasks;
using Comms.Core.Queries;
using Comms.Core.Services;
using Comms.Core.SMS;
using Comms.Domain;

using Library.Service.Api;

namespace Comms.Api;

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
        app.UsePathBase("/comms");
        app.ConfigureStartup(Configuration, env, true);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.RegisterMassTransit("comms", Configuration);

        // Add auth
        services.RegisterAuth(Configuration);

        // Inject settings
        services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
        services.Configure<TextMagicSettings>(Configuration.GetSection("TextMagicSettings"));

        // Inject services
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<INotificationService, NotificationService>();

        // Inject queries
        services.AddTransient<INotificationQueries, NotificationQueries>();

        // Add sms provider
        services.AddSingleton<ISmsProvider, TextMagicSmsProvider>();

        // Add database
        services.AddDatabase<CommsContext>(Configuration, typeof(Startup).Assembly.GetName().Name);

        // Register background tasks
        services.AddHostedService<SendEmailsTask>();
    }
}