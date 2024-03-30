using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oakton;
using System.Threading.Tasks;
using Microsoft.AspNetCore;

namespace Kafe.Api;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var host = builder.Build();
        return await host.RunOaktonCommands(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.ConfigureLogging(l =>
                {
                    l.ClearProviders();
                    l.AddSimpleConsole();
                });
                builder.ConfigureKestrel(k =>
                {
                    // set request limit to 4 GiB
                    k.Limits.MaxRequestBodySize = Const.ShardSizeLimit;
                });
                builder.ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.local.json");
                    c.AddEnvironmentVariables();
                });
                builder.UseStartup<Startup>();
            })
            .ApplyOaktonExtensions();
    }
}
