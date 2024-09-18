using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Coordination;
using Marten.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

public class ApiTestBase : IAsyncLifetime
{
    private readonly ITestOutputHelper testOutput;

    public ApiTestBase(ApiFixture fixture, ITestOutputHelper testOutput)
    {
        Host = fixture.Host;
        Store = fixture.Host.Server.Services.GetRequiredService<IDocumentStore>();
        this.testOutput = testOutput;
    }

    public IAlbaHost Host { get; }
    public IDocumentStore Store { get; }
    public IProjectionCoordinator? ProjectionCoordinator { get; private set; }

    public async Task InitializeAsync()
    {
        var outputSink = Host.Server.Services.GetRequiredService<IInjectableTestOutputSink>();
        outputSink.Inject(testOutput);
        await Store.Advanced.ResetAllData();
        ProjectionCoordinator = Host.Server.Services.GetRequiredService<IProjectionCoordinator>();
        await ProjectionCoordinator.DaemonForMainDatabase().StartAllAsync();

    }

    public async Task DisposeAsync()
    {
        if (ProjectionCoordinator is not null)
        {
            await ProjectionCoordinator.DaemonForMainDatabase().StopAllAsync();
        }
    }

    public async Task WaitForProjections()
    {
        await Store.WaitForNonStaleProjectionDataAsync(TimeSpan.FromMinutes(1));
    }
}
