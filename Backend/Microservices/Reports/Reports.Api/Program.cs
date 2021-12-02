using Library.Service.Api;

using Microsoft.AspNetCore;

using Reports.Domain;

namespace Reports.Api;

public static class Program
{
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).CreateBuilder<Startup>();

    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().MigrateDbContext<ReportContext>().Run();
    }
}