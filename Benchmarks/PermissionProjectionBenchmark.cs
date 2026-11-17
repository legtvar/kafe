using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Kafe.Data.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kafe.Benchmarks;

/// <remarks>Run this against an isolated database copy. The benchmark rebuilds projection data.</remarks>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class PermissionProjectionBenchmark
{
    private IHost host = null!;
    private IDocumentStore store = null!;

    [GlobalSetup]
    public void Setup()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:KAFE"] = Environment.GetEnvironmentVariable("KAFE_CONNECTION_STRING")
                ?? "Host=localhost;Database=kafe;Username=postgres;Password=postgres;Include Error Detail=true",
            ["ConnectionStrings:postgres"] = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? "Host=localhost;Database=postgres;Username=postgres;Password=postgres",
            ["Storage:SecretsDirectory"] = ".",
            ["Storage:TempDirectory"] = ".",
            ["Storage:ArchiveDirectory"] = ".",
            ["Storage:GeneratedDirectory"] = ".",
            ["Logging:LogLevel:Default"] = "Warning"
        };
        host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => c.AddInMemoryCollection(settings))
            .ConfigureServices(s => s.AddKafeData())
            .Build();
        store = host.Services.GetRequiredService<IDocumentStore>();
    }

    [GlobalCleanup]
    public void Cleanup() => host.Dispose();

    [Benchmark]
    public async Task RebuildPermissionProjection()
    {
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<EntityPermissionEventProjection>(CancellationToken.None);
    }
}
