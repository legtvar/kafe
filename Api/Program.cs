using Microsoft.AspNetCore.Builder;

namespace Kafe.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder => {
                builder.ConfigureAppConfiguration(c => {
                   c.AddJsonFile("appsettings.local.json");
                });
                builder.UseStartup<Startup>();
            });

        var host = builder.Build();
        host.Run();
    }
}
