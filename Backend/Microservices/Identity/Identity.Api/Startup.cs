
using Identity.Api.Requirements;
using Identity.Core;
using Identity.Core.Queries;
using Identity.Core.Services;
using Identity.Domain;

using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;

namespace Identity.Api;

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
        app.ConfigureStartup(Configuration, env, true);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared services
        services.AddAutoMapper(typeof(Startup));
        services.RegisterServices(Configuration);
        services.RegisterMassTransit("identity", Configuration);

        // Add auth
        services.AddTransient<IAuthorizationHandler, CheckUserAuthorizeHandler>();
        var policies = new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
        {
            {  "CheckUser", (policy) => policy.Requirements.Add(new CheckUserRequirement()) }
        };
        services.RegisterAuth(Configuration, policies);

        // Inject settings
        services.Configure<IdentitySettings>(Configuration.GetSection("IdentitySettings"));

        // Inject services
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<ITwoFactorService, TwoFactorService>();
        services.AddTransient<IUserKeyService, UserKeyService>();

        // Inject queries
        services.AddTransient<IUserQueries, UserQueries>();
        services.AddTransient<IAuthTokenQueries, AuthTokenQueries>();

        // Add identity db
        services.AddDatabase<IdentityContext>(Configuration, typeof(Startup).Assembly.GetName().Name);
    }
}