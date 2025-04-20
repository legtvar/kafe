using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Marten;

namespace Kafe.Data.Services;

public class EntityService
{
    private readonly IKafeDocumentSession db;

    public EntityService(IKafeDocumentSession db)
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
        return await db.LoadAsync<EntityPermissionInfo>(entityId, token);
    }

    public async Task<Err<ImmutableArray<EntityPermissionInfo>>> LoadPermissionInfoMany(
        IEnumerable<Hrib> entityIds,
        CancellationToken token = default
    )
    {
        return await db.LoadManyAsync<EntityPermissionInfo>([.. entityIds], token);
    }

    public async Task<Permission> GetPermission(
        Hrib entityId,
        Hrib accessingAccountId,
        CancellationToken token = default
    )
    {
        var perms = await LoadPermissionInfo(entityId, token);
        // NB: Special case: The async deamon might still be processing EntityPermissionEventProjection, so we look into
        //     the account itself.
        if (perms.HasError)
        {
            if (perms.Diagnostic.Payload.Value is NotFoundDiagnostic)
            {
                var accessingAccount = (await db.LoadAsync<AccountInfo>(accessingAccountId, token)).Unwrap();
                return accessingAccount.Permissions.GetValueOrDefault(entityId.ToString());
            }

            throw perms.AsException();
        }

        return accessingAccountId.IsEmpty
            ? perms.Value.GlobalPermission.EffectivePermission
            : perms.Value.GetAccountPermission(accessingAccountId) | perms.Value.GlobalPermission.EffectivePermission;
    }

    public async Task<ImmutableArray<Permission>> GetPermissions(
        IEnumerable<Hrib> entityIds,
        Hrib accessingAccountId,
        CancellationToken token = default
    )
    {
        if (entityIds.IsEmpty())
        {
            return ImmutableArray<Permission>.Empty;
        }

        var manyPerms = (await LoadPermissionInfoMany(entityIds, token)).Unwrap();
        return manyPerms.Select(p => accessingAccountId.IsEmpty
                ? p.GlobalPermission.EffectivePermission
                : p.GetAccountPermission(accessingAccountId) | p.GlobalPermission.EffectivePermission)
            .ToImmutableArray();
    }

    // TODO: Refactor into SetAccountPermissions, SetRolePermissions, and SetGlobalPermissions.
    //       This is less than intuitive.
    public async Task SetPermissions(
        Hrib entityId,
        Permission permissions,
        Hrib accessingAccountId,
        CancellationToken token = default)
    {
        if (accessingAccountId.IsEmpty)
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
                    OrganizationInfo o => new OrganizationGlobalPermissionsChanged(o.Id, permissions),
                    _ => throw new NotSupportedException($"{entity.GetType().Name} is not a supported entity type.")
                };

                db.Events.Append(entityId.ToString(), newEvent);
            }

        }
        else
        {
            var account = (await db.LoadAsync<AccountInfo>(accessingAccountId, token)).Unwrap();

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
