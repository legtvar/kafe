using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using Marten;
using Marten.Events.Daemon.Coordination;
using Marten.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

public class ApiContext : IAsyncLifetime
{
    private readonly ITestOutputHelper testOutput;

    public ApiContext(ApiFixture fixture, ITestOutputHelper testOutput)
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

    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task WaitForProjections()
    {
        var projectionsCount = ((MartenDatabase)Store.Storage.Database).Options.Projections.AllShards().Count + 1;
        var stats = await Store.Advanced.FetchEventStoreStatistics();
        while(true)
        {
            var progress = await Store.Storage.Database.AllProjectionProgress();
            if (progress.Count >= projectionsCount && progress.All(p => p.Sequence >= stats.EventSequenceNumber))
            {
                return;
            }

            await Task.Delay(250);
        }
    }
}
