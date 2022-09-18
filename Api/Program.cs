using Microsoft.AspNetCore.Builder;

namespace Kafe.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder => {
                builder.UseStartup<Startup>();
            });

        var host = builder.Build();
        host.Run();
    }
}
