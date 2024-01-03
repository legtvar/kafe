using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
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
        var parentState = await db.Events.FetchStreamStateAsync(id.ToString(), token);
        if (parentState?.AggregateType is null)
        {
            return null;
        }

        return (await db.QueryAsync(
            parentState.AggregateType,
            "WHERE data ->> 'Id' = ?",
            token,
            parentState.Key))
                .Cast<IEntity>()
                .FirstOrDefault();
    }

    // public async Task<ImmutableArray<bool>> AllExist<TEntity>(IEnumerable<Hrib> ids, CancellationToken token = default)
    // {
    //     var tableName = await db.Database.ExistingTableFor(typeof(TEntity));
    //     if (tableName )
    // }

    public async Task<Permission> GetPermission(
        Hrib entityId,
        Hrib? accessingAccountId = null,
        CancellationToken token = default)
    {
        var perms = await db.QueryAsync<int>(
            $"SELECT {db.DocumentStore.Options.DatabaseSchemaName}.{SqlFunctions.GetResourcePerms}(?, ?)",
            token,
            entityId.ToString(),
            accessingAccountId?.ToString()!);
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
            $"SELECT {db.DocumentStore.Options.DatabaseSchemaName}.{SqlFunctions.GetResourcePerms}(id, ?) "
            + "FROM unnest(?) as id",
            token,
            accessingAccountId?.ToString()!,
            entityIds.Select(i => i.ToString()).ToArray()
        );
        return perms.Select(p => (Permission)p).ToImmutableArray();
    }

    public async Task SetPermissions(
        Hrib entityId,
        Permission permissions,
        Hrib? accessingAccountId = null,
        CancellationToken token = default)
    {
        if (accessingAccountId is null)
        {
            if (entityId == Hrib.System)
            {
                throw new InvalidOperationException("System permissions cannot be set globally.");
            }

            var entity = await Load(entityId, token) ?? throw new NullReferenceException("Entity could not be found.");

            if (entity is not IVisibleEntity visible)
            {
                throw new InvalidOperationException("Entity cannot have global permissions.");
            }

            if (visible.GlobalPermissions != permissions)
            {
                object newEvent = entity switch
                {
                    ProjectGroupInfo pg => new ProjectGroupGlobalPermissionsChanged(pg.Id, permissions),
                    ProjectInfo p => new ProjectGlobalPermissionsChanged(p.Id, permissions),
                    PlaylistInfo p => new PlaylistGlobalPermissionsChanged(p.Id, permissions),
                    AuthorInfo a => new AuthorGlobalPermissionsChanged(a.Id, permissions),
                    _ => throw new NotSupportedException($"{entity.GetType().Name} is not a supported entity type.")
                };
                
                db.Events.Append(entityId.Value, newEvent);
            }

        }
        else
        {
            var account = await db.LoadAsync<AccountInfo>(accessingAccountId.ToString(), token)
                ?? throw new NullReferenceException("Account could not be found.");

            if (entityId != Hrib.System)
            {
                _ = await Load(entityId, token) ?? throw new NullReferenceException("Entity could not be found.");
            }

            if ((account.Permissions?.GetValueOrDefault(entityId.ToString()) ?? Permission.None) != permissions)
            {
                object @event = permissions != Permission.None
                    ? new AccountPermissionSet(
                        AccountId: accessingAccountId.ToString(),
                        EntityId: entityId.ToString(),
                        Permission: permissions)
                    : new AccountPermissionUnset(
                        AccountId: accessingAccountId.ToString(),
                        EntityId: entityId.ToString());
                db.Events.Append(accessingAccountId.ToString(), @event);
            }

        }

        await db.SaveChangesAsync(token);
    }

}
