using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Documents;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafe.Data;

public class SystemSeedData : IInitialData
{
    private readonly IServiceProvider services;
    private readonly ILogger<SystemSeedData> logger;

    public SystemSeedData(
        IServiceProvider services,
        ILogger<SystemSeedData> logger)
    {
        this.services = services;
        this.logger = logger;
    }

    public async Task Populate(IDocumentStore store, CancellationToken token)
    {
        using var scope = services.CreateScope();
        using var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();

        var systemPerms = await session.KafeLoadAsync<EntityPermissionInfo>(Hrib.System, token);
        if (systemPerms.HasErrors)
        {
            session.Store(EntityPermissionInfo.Create(Hrib.System));
        }

        logger.LogInformation("Applying system seed data.");
        await session.SaveChangesAsync(token);
    }
}
