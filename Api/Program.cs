using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using JasperFx;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Kafe.Api;

public class Program
{
    public static readonly ExpressionTemplate LogTemplate
        = new(
            "[{@t:HH:mm:ss.fff} {@l:u3} {SourceContext}{#if Scope is not null and Length(Scope) > 0}: {Scope[0]}{#end}]\n"
            + "{@m}\n"
            + "{@x}",
            theme: TemplateTheme.Code
        );

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
        return await host.RunJasperFxCommands(args);
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
                        }
                    );
                    builder.ConfigureAppConfiguration(c =>
                        {
                            c.AddJsonFile("appsettings.local.json");
                            c.AddUserSecrets(typeof(Program).Assembly);
                            c.AddEnvironmentVariables();
                        }
                    );
                    builder.UseStartup<Startup>();
                }
            )
            .ApplyJasperFxExtensions();
    }
}
