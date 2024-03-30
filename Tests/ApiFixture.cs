using System;
using System.Threading.Tasks;
using Alba;
using Kafe.Data.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kafe.Tests;

public class ApiFixture : IAsyncLifetime
{
    private readonly string testSchema = "test" + Guid.NewGuid().ToString().Replace("-", string.Empty);

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Host = await AlbaHost.For<Api.Program>(b =>
        {
            b.ConfigureServices((context, services) =>
            {
                services.Configure<StorageOptions>(o =>
                {
                    o.Schema = testSchema;
                });
            });
        });
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
