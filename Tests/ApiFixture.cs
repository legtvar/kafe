using System;
using System.Threading;
using System.Threading.Tasks;
using Alba;
using Alba.Security;
using Kafe.Data.Options;
using Marten;
using Marten.Events.Daemon.Coordination;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

public class ApiFixture(IMessageSink diagnosticSink) : IAsyncLifetime
{
    public const string TestLogTemplate
        = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";

    private readonly string testSchema
        = $"test_{DateTimeOffset.UtcNow:yyyy_MM_dd_T_HH_mm}_{Guid.NewGuid().ToString()[..8]}";

    public IAlbaHost Host { get; private set; } = null!;
    public IMessageSink DiagnosticSink { get; } = diagnosticSink;

    public async Task InitializeAsync()
    {
        var authenticationStub = new AuthenticationStub();
        Host = await AlbaHost.For<Api.Program>(b =>
        {
            b.ConfigureServices((context, services) =>
            {
                var injectableSink = new InjectableTestOutputSink(
                    outputTemplate: TestLogTemplate
                );
                services.AddSingleton<IInjectableTestOutputSink>(injectableSink);
                services.AddSerilog((sp, lc) => lc
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(sp)
                    .MinimumLevel.Override("Marten", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.InjectableTestOutput(injectableSink)
                );
                services.MartenDaemonModeIsSolo();
                services.InitializeMartenWith<TestSeedData>();
                services.Configure<StorageOptions>(o =>
                {
                    o.AllowSeedData = false;
                    o.Schema = testSchema;
                });
            });
        }, authenticationStub);
        // NB: Let the test start the daemon
        // var coordinator = (ProjectionCoordinator)Host.Server.Services.GetRequiredService<IProjectionCoordinator>();
        // await coordinator.PauseAsync();
    }

    public async Task DisposeAsync()
    {
        if (Host is not null)
        {
            await Host.DisposeAsync();
        }
    }
}
