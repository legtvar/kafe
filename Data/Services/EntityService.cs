using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Marten;

namespace Kafe.Data.Services;

public class EntityService
{
    private readonly IDocumentSession db;

    public EntityService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<IEntity?> Load(Hrib id, CancellationToken token = default)
    {
        var parentState = await db.Events.FetchStreamStateAsync(id.Value, token);
        if (parentState?.AggregateType is null)
        {
            return null;
        }

        return (await db.QueryAsync(
            parentState.AggregateType,
            "where data ->> Id = ?",
            token,
            parentState.Id))
                .Cast<IEntity>()
                .FirstOrDefault();
    }
    
    public async Task<Permission> GetPermission(
        Hrib entityId,
        Hrib? accessingAccountId = null,
        CancellationToken token = default)
    {
        var perms = await db.QueryAsync<int>(
            $"SELECT {SqlFunctions.GetResourcePerms}(?, ?)",
            token,
            entityId.Value,
            accessingAccountId?.Value!);
        return (Permission)perms.Single();
    }
    
    public async Task<ImmutableArray<Permission>> GetPermissions(
        IEnumerable<Hrib> entityIds,
        Hrib? accessingAccountId = null,
        CancellationToken token = default)
    {
        if (entityIds.IsEmpty())
        {
            return ImmutableArray<Permission>.Empty;
        }

        var perms = await db.QueryAsync<int>(
            $"SELECT {SqlFunctions.GetResourcePerms}(id, ?) FROM unnest(?) as id",
            token,
            accessingAccountId?.Value!,
            entityIds.Select(i => i.Value).ToArray()
        );
        return perms.Select(p => (Permission)p).ToImmutableArray();
    }
}
