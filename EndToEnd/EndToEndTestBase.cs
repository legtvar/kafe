using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.EndToEnd;

public class EndToEndTestBase(EndToEndFixture fixture, ITestOutputHelper testOutput) : IAsyncLifetime
{
    public ILogger? Log { get; private set; }

    public Task InitializeAsync()
    {
        Log = Serilog.Log.Logger;
        Log.Information("Initializing.");

        var outputSink = new InjectableTestOutputSink();
        outputSink.Inject(testOutput, fixture.DiagnosticSink);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Log?.Information("Disposal complete.");
        return Task.CompletedTask;
    }
}
