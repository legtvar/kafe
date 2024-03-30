using System.Threading.Tasks;
using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kafe.Tests;

public class ApiContext : IAsyncLifetime
{
    public ApiContext(ApiFixture fixture)
    {
        Host = fixture.Host;
        Store = fixture.Host.Server.Services.GetRequiredService<IDocumentStore>();
    }

    public IAlbaHost Host { get; }
    public IDocumentStore Store { get; }

    public async Task InitializeAsync()
    {
        await Store.Advanced.ResetAllData();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
