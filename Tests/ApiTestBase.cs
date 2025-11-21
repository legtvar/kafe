using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alba;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Coordination;
using Marten.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

public class ApiTestBase : IAsyncLifetime
{
    private readonly ApiFixture fixture;
    private readonly ITestOutputHelper testOutput;

    public ApiTestBase(ApiFixture fixture, ITestOutputHelper testOutput)
    {
        Host = fixture.Host;
        Store = fixture.Host.Server.Services.GetRequiredService<IDocumentStore>();
        this.fixture = fixture;
        this.testOutput = testOutput;
    }

    public IAlbaHost Host { get; }
    public IDocumentStore Store { get; }
    public IProjectionCoordinator? ProjectionCoordinator { get; private set; }
    public ILogger? Log { get; private set; }

    public async Task InitializeAsync()
    {
        Log = Host.Server.Services.GetRequiredService<ILogger>().ForContext(GetType());
        Log.Information("Initializing.");

        var outputSink = Host.Server.Services.GetRequiredService<IInjectableTestOutputSink>();
        outputSink.Inject(testOutput, fixture.DiagnosticSink);

        ProjectionCoordinator = Host.Server.Services.GetRequiredService<IProjectionCoordinator>();
        await ProjectionCoordinator.PauseAsync();

        await Store.Advanced.ResetAllData();

        await ProjectionCoordinator.ResumeAsync();

        await WaitForProjections();

        Log.Information("Initialization complete.");
    }

    public async Task DisposeAsync()
    {
        if (ProjectionCoordinator is not null)
        {
            await ProjectionCoordinator.PauseAsync();
        }

        Log?.Information("Disposal complete.");
    }

    public async Task WaitForProjections()
    {
        await Store.WaitForNonStaleProjectionDataAsync(TimeSpan.FromMinutes(4));
    }
}
