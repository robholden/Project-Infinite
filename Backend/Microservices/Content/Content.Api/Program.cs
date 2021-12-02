
using Content.Domain;

using Library.Service.Api;

using Microsoft.AspNetCore;

namespace Content.Api;

public static class Program
{
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).CreateBuilder<Startup>();

    public static async Task Main(string[] args)
    {
        await CreateWebHostBuilder(args).Build().MigrateDbContext<ContentContext>(ctx => ctx.Seed()).RunAsync();
    }
}