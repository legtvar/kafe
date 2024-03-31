using System;
using System.Threading.Tasks;
using Alba;
using Alba.Security;
using Kafe.Data.Options;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kafe.Tests;

public class ApiFixture : IAsyncLifetime
{
    private readonly string testSchema = "test" + Guid.NewGuid().ToString().Replace("-", string.Empty);

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var authenticationStub = new AuthenticationStub();
        Host = await AlbaHost.For<Api.Program>(b =>
        {
            b.ConfigureServices((context, services) =>
            {
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
