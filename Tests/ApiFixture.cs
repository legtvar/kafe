using System;
using System.Threading.Tasks;
using Alba;
using Alba.Security;
using Kafe.Data.Options;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

public class ApiFixture : IAsyncLifetime
{
    private readonly string testSchema
        = $"test_{DateTimeOffset.UtcNow:yyyy_MM_dd_T_HH_mm}_{Guid.NewGuid().ToString()[..8]}";

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var authenticationStub = new AuthenticationStub();
        Host = await AlbaHost.For<Api.Program>(b =>
        {
            b.ConfigureServices((context, services) =>
            {
                var injectableSink = new InjectableTestOutputSink(
                    outputTemplate: Api.Program.LogTemplate);
                services.AddSingleton<IInjectableTestOutputSink>(injectableSink);
                services.AddSerilog((sp, lc) => lc
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(sp)
                    .Enrich.FromLogContext()
                    .WriteTo.InjectableTestOutput(injectableSink)
                );
                services.InitializeMartenWith<TestSeedData>();
                services.Configure<StorageOptions>(o =>
                {
                    o.AllowSeedData = false;
                    o.Schema = testSchema;
                });
            });
        }, authenticationStub);
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
