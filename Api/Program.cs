using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oakton;
using System.Threading.Tasks;

namespace Kafe.Api;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.ConfigureLogging(l =>
                {
                    l.ClearProviders();
                    l.AddSimpleConsole(c =>
                    {
                        c.IncludeScopes = true;
                    });
                });
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
            })
            .ApplyOaktonExtensions();

        var host = builder.Build();
        return await host.RunOaktonCommands(args);
    }
}
