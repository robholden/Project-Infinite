using Library.Service.Api;

using Microsoft.AspNetCore;

namespace Sockets;

public static class Program
{
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).CreateBuilder<Startup>();

    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }
}