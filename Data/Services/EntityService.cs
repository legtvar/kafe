using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
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

    public async Task<Err<EntityPermissionInfo>> LoadPermissionInfo(
        Hrib entityId,
        CancellationToken token = default
    )
    {
        return await db.KafeLoadAsync<EntityPermissionInfo>(entityId, token);
    }
    
    public async Task<Err<ImmutableArray<EntityPermissionInfo>>> LoadPermissionInfoMany(
        IEnumerable<Hrib> entityIds,
        CancellationToken token = default
    )
    {
        return await db.KafeLoadManyAsync<EntityPermissionInfo>(entityIds.ToImmutableArray(), token);
    }

    public async Task<Permission> GetPermission(
        Hrib entityId,
        Hrib? accessingAccountId = null,
        CancellationToken token = default
    )
    {
        var perms = (await LoadPermissionInfo(entityId, token)).Unwrap();
        return accessingAccountId is null
            ? perms.GlobalPermission
            : perms.GetAccountPermission(accessingAccountId);
    }

    public async Task<ImmutableArray<Permission>> GetPermissions(
        IEnumerable<Hrib> entityIds,
        Hrib? accessingAccountId = null,
        CancellationToken token = default
    )
    {
        if (entityIds.IsEmpty())
        {
            return ImmutableArray<Permission>.Empty;
        }
        
        var manyPerms = (await LoadPermissionInfoMany(entityIds, token)).Unwrap();
        return manyPerms.Select(p => accessingAccountId is null
                ? p.GlobalPermission
                : p.GetAccountPermission(accessingAccountId))
            .ToImmutableArray();
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

                db.Events.Append(entityId.ToString(), newEvent);
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
                object @event = new AccountPermissionSet(
                        AccountId: accessingAccountId.ToString(),
                        EntityId: entityId.ToString(),
                        Permission: permissions);
                db.Events.Append(accessingAccountId.ToString(), @event);
            }

        }

        await db.SaveChangesAsync(token);
    }

}
