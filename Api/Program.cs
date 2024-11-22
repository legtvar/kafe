using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Oakton;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace Kafe.Api;

public class Program
{
    public const string LogTemplate
        = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";
    public const string BootstrapLogTemplate
        = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext} (bootstrap)]{NewLine}{Message:lj}{NewLine}{Exception}";

    public static async Task<int> Main(string[] args)
    {
        // NB: The logger below is used just during bootstrapping.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: BootstrapLogTemplate
            )
            .CreateBootstrapLogger();


        var builder = CreateHostBuilder(args);
        var host = builder.Build();
        return await host.RunOaktonCommands(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.ConfigureKestrel(k =>
                {
                    // set request limit to 4 GiB
                    k.Limits.MaxRequestBodySize = Const.ShardSizeLimit;
                    k.AddServerHeader = false;
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
