
using System.Net;

using Library.Service;
using Library.Service.Api;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace Gateway;

public class Startup
{
    private static readonly string HeaderRecaptchaToken = "x-recaptcha-token";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);

        services.AddAntiforgery(options => options.HeaderName = "x-csrf-token");

        services.AddCors();
        services.AddOcelot().AddConsul();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SharedSettings> sharedOptions, IAntiforgery antiforgery)
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

        app.ConfigureStartup(
            withAuth: false,
            routeBuilder: endpoints => endpoints.UseHealthChecks((HttpContext context) =>
            {
                // Only send forgery token for browser applications
                if (!context.IsCapacitor())
                {
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                }

                context.Response.Headers.Add("Access-Control-Expose-Headers", HeaderRecaptchaToken);
                context.Response.Headers.Add(HeaderRecaptchaToken, sharedOptions.Value.ReCaptchaClientKey);
            }),
            useDefaultHealthCheck: false
        );

        // Header checker middleware
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "";
            if (path.StartsWith("/api"))
            {
                // Only verify forgery token for browser applications
                if (!context.IsCapacitor() && !await antiforgery.IsRequestValidAsync(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Danger! This is a potentially dangerous request.");
                    return;
                }
            }

            // Call the next delegate/middleware in the pipeline
            await next();
        });

        // Enable web sockets
        app.UseWebSockets();

        // Start ocelot
        await app.UseOcelot();
    }
}
