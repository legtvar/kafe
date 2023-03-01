using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Kafe.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.ConfigureKestrel(k =>
                {
                    // set request limit to 4 GiB
                    k.Limits.MaxRequestBodySize = Const.ShardSizeLimit;
                });
                builder.ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.local.json");
                });
                builder.UseStartup<Startup>();
            });

        var host = builder.Build();
        host.Run();
    }
}
