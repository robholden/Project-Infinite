using System.Text;
using System.Text.Json;

using Library.Core;
using Library.Service.Api.Auth;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Library.Service.Api;

public static class StartupExtensions
{

    public static void ConfigureStartup(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env, bool withAuth, Action<IEndpointRouteBuilder> routeBuilder = null)
    {
        if (env.IsDevMode()) app.UseDeveloperExceptionPage();

        app.UseCors(x => x
            .BuildOrigins(config["AllowedOrigins"])
            .AllowAnyMethod()
            .AllowAnyHeader()
        );

        app.UseRouting();

        if (withAuth)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseEndpoints(endpoints =>
        {
            routeBuilder?.Invoke(endpoints);
            endpoints.MapControllers();

            endpoints.UseHealthChecks();
        });
    }

    public static void UseHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new()
        {
            ResponseWriter = (HttpContext context, HealthReport result) =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";

                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new() { Indented = true }))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", result.Status.ToString());
                    writer.WriteStartObject("results");
                    foreach (var entry in result.Entries)
                    {
                        writer.WriteStartObject(entry.Key);
                        writer.WriteString("status", entry.Value.Status.ToString());
                        writer.WriteString("description", entry.Value.Description);
                        writer.WriteStartObject("data");
                        foreach (var item in entry.Value.Data)
                        {
                            writer.WritePropertyName(item.Key);
                            JsonSerializer.Serialize(writer, item.Value, item.Value?.GetType() ?? typeof(object));
                        }
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                return context.Response.WriteAsync(json);
            }
        });
    }

    public static void AddDatabase<T>(this IServiceCollection services, IConfiguration config, string assembly = null, bool lazyLoad = false) where T : DbContext
    {
        // Null set assembly
        assembly ??= typeof(T).Assembly.GetName().Name;

        // Add db context
        var connectionString = config.GetConnectionString("Database") ?? throw new Exception("Connection string not found for Database!");

        // SqlServer
        try
        {
            services
                .AddDbContext<T>(x => x
                    .EnableDetailedErrors(false)
                    .UseSqlServer(connectionString, options => options.MigrationsAssembly(assembly))
                    //.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options => options.MigrationsAssembly(assembly))
                    .UseLazyLoadingProxies(lazyLoad)
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to inject database: {0}", ex.Message);
            Thread.Sleep(2500);
            throw;
        }
    }

    public static AuthenticationBuilder RegisterAuth(this IServiceCollection services, IConfiguration configuration, IDictionary<string, Action<AuthorizationPolicyBuilder>> policies = null)
    {
        // Replace the default authorization policy provider with our own custom provider
        // which can return authorization policies for given policy names (instead of using the default policy provider)
        services.AddSingleton<IAuthorizationPolicyProvider, IdentityPolicyProvider>();

        // As always, handlers must be provided for the requirements of the authorization policies
        services.AddTransient<IAuthorizationHandler, IdentityAuthorizeHandler>();

        // Add custom provider for SignalR purposes to use custom claim for identification
        services.AddSingleton<IUserIdProvider, UserIdBasedUserIdProvider>();

        // Add tracker for limit requests
        services.AddScoped<LimitRequestTracker>();

        // Configure jwt authentication
        var sharedKeySection = configuration.GetSection("Shared");
        var sharedKeys = sharedKeySection.Get<SharedSettings>();
        var builder = services
             .AddAuthorization(opts =>
             {
                 if (policies?.Any() == true)
                 {
                     foreach (var policy in policies) opts.AddPolicy(policy.Key, policy.Value);
                 }
             })
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.IncludeErrorDetails = true;
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(sharedKeys.JwtIssuerKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Auth di
        services.AddHttpContextAccessor();
        services.AddTransient<IReCaptchaService, ReCaptchaService>();

        return builder;
    }

    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add things
        services.AddHealthChecks();

        // Add controller and Newtonsoft
        services
            .AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        // Disable automatic state validation
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        // Shared keys
        services.Configure<SharedSettings>(configuration.GetSection("Shared"));

        // Add api protection
        services.AddTransient<ApiResponseFilter>();

        // Add caching
        services.AddMemoryCache();
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn)) services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
    }

    public static void RegisterMassTransit(this IServiceCollection services, string queueName, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            var assemblyTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            var types = assemblyTypes.Where(p => typeof(ISnowConsumer).IsAssignableFrom(p) && p.Name != nameof(ISnowConsumer));
            foreach (var consumer in types) x.AddConsumer(consumer);

            x.UsingRabbitMq((context, cfg) =>
            {
                // Set RabbitMq host address from settings
                var host = configuration["RabbitMq:Hostname"] ?? "localhost";
                cfg.Host($"rabbitmq://{ host }", h =>
                {
                    var username = configuration["RabbitMq:Username"];
                    var password = configuration["RabbitMq:Password"];

                    if (!string.IsNullOrEmpty(username)) h.Username(username);
                    if (!string.IsNullOrEmpty(password)) h.Password(password);
                });

                // Do not register endpoint if there's no consumers registered
                if (!types.Any())
                {
                    return;
                }

                // Get queue name from assembly
                if (string.IsNullOrEmpty(queueName)) return;

                // Map queues with consumers
                cfg.ReceiveEndpoint(queueName, e =>
                {
                    foreach (var consumer in types) e.ConfigureConsumer(context, consumer);
                });
            });

            // Add pub subs
            services.AddPubSubs(x);
        }).AddMassTransitHostedService();
    }

    public static CorsPolicyBuilder BuildOrigins(this CorsPolicyBuilder builder, string origins)
    {
        origins ??= "*";
        return origins == "*" ? builder.AllowAnyOrigin() : builder.WithOrigins(origins.Split(",")).AllowCredentials();
    }

    public static IWebHostBuilder CreateBuilder<T>(this IWebHostBuilder builder, Action<WebHostBuilderContext, IConfigurationBuilder> onConfig = null) where T : class
    {
        return builder
             .UseKestrel()
             .UseContentRoot(Directory.GetCurrentDirectory())
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config
                     .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                     .AddJsonFile("appsettings.json", false, true)
                     .AddJsonFile($"appsettings.{ hostingContext.HostingEnvironment.EnvironmentName }.json", true, true)
                     .AddJsonFile("appsettings.local.json", true, true)
                     .AddUserSecrets<T>(optional: true)
                     .AddEnvironmentVariables();

                 onConfig?.Invoke(hostingContext, config);
             })
             .ConfigureLogging((WebHostBuilderContext hostingContext, ILoggingBuilder logging) =>
             {
                 var section = hostingContext.Configuration.GetSection("Logging");

                 logging.AddConfiguration(section);
                 logging.AddConsole();
                 logging.AddDebug();
                 logging.AddEventSourceLogger();

                 if (section.GetSection("LogToFile").Get<bool?>() == true)
                 {
                     logging.AddFile(section);
                 }

                 logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
             })
             .UseStartup<T>()
             .UseIIS();
    }

    public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext> seeder = null) where TContext : DbContext
    {
        using var scope = webHost.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        var context = scope.ServiceProvider.GetService<TContext>();

        // Only migrate on valid providers
        if (context.Database.ProviderName.Contains("InMemory"))
        {
            return webHost;
        }

        try
        {
            // Apply migration
            context.Database.Migrate();

            // Run seeder
            seeder?.Invoke(context);
        }
        catch
        {
            logger.LogError("ConnString={0}", context.Database.GetConnectionString());
            throw;
        }

        return webHost;
    }
}