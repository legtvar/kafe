using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace Kafe.Data.Projections;

public class EntityPermissionEventProjection : EventProjection
{
    public async Task<EntityPermissionInfo> Create(OrganizationCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.OrganizationId);
        perms = await AddSystem(ops, perms);
        return perms;
    }

    public async Task<EntityPermissionInfo> Create(ProjectGroupCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.ProjectGroupId);
        // NB: Since the project group can technically have no parent organization at this point,
        //     we must add system perms explicitly.
        perms = await AddSystem(ops, perms);

        if (e.OrganizationId is not null)
        {
            perms = await AddParent(ops, perms, e.OrganizationId);
        }

        return perms;
    }

    public async Task<EntityPermissionInfo> Create(ProjectCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.ProjectId);
        perms = await AddSystem(ops, perms);
        // NB: We assume that the project group's perms are complete, thus we don't need to dig further to the org.
        perms = await AddParent(ops, perms, e.ProjectGroupId);

        return perms;
    }

    public async Task<EntityPermissionInfo> Create(PlaylistCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.PlaylistId);
        perms = await AddSystem(ops, perms);
        if (e.OrganizationId is not null)
        {
            perms = await AddParent(ops, perms, e.OrganizationId);
        }

        return perms;
    }

    public async Task<EntityPermissionInfo> Create(AuthorCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.AuthorId);
        perms = await AddSystem(ops, perms);
        return perms;
    }

    public async Task<EntityPermissionInfo> Create(ArtifactCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.ArtifactId);
        perms = await AddSystem(ops, perms);
        return perms;
    }

    public async Task<EntityPermissionInfo> Create(AccountCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.AccountId);
        perms = await AddSystem(ops, perms);
        return perms;
    }

    public async Task Project(RoleCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.RoleId);
        perms = await AddSystem(ops, perms);
        ops.Store(perms);

        var roleMembers = RoleMembersInfo.Create(e.RoleId);
        ops.Store(roleMembers);
    }

    public async Task Project(AccountPermissionSet e, IEvent metadata, IDocumentOperations ops)
    {
        // NB: Once upon a time, we used "*" to identify the whole system. Then we changed it, but we've got total of
        //     THREE AccountPermissionSet events in the DB that would break this method. Implementing an upcast that
        //     would have to run on any past or future AccountPermissionSet seems inefficient so I just added this `if`.
        //     We all have to live with our mistakes.
        if (e.EntityId == "*")
        {
            e = e with
            {
                EntityId = Hrib.SystemValue
            };
        }

        var entityPerms = await RequireEntityPermissionInfo(ops, e.EntityId);
        entityPerms = entityPerms with
        {
            AccountEntries = SetPermission(
            entries: entityPerms.AccountEntries,
            accountOrRoleId: e.AccountId,
            // NB: Explicit permissions have source Id equal to the entity Id.
            //     This makes them easy to find and allows for consistent inheriting of info about perms sources.
            sourceId: e.EntityId,
            permission: e.Permission,
            grantedAt: metadata.Timestamp)
        };
        ops.Store(entityPerms);

        var inheritedPermission = InheritPermission(e.Permission);
        // NB: Since we don't know if we're adding or removing permissions we have to try to update all of the entity's
        //     descendants.
        await SpreadAccountPermission(ops, [e.AccountId], e.EntityId, inheritedPermission, metadata.Timestamp);
    }

    public async Task Project(RolePermissionSet e, IEvent metadata, IDocumentOperations ops)
    {
        var entityPerms = await RequireEntityPermissionInfo(ops, e.EntityId);
        var roleInfo = await RequireRoleMembersInfo(ops, e.RoleId);

        entityPerms = entityPerms with
        {
            RoleEntries = SetPermission(
                entries: entityPerms.RoleEntries,
                accountOrRoleId: e.RoleId,
                // NB: Explicit permissions have source Id equal to the entity Id.
                //     This makes them easy to find and allows for consistent inheriting of info about perms sources.
                sourceId: e.EntityId,
                permission: e.Permission,
                grantedAt: metadata.Timestamp
            )
        };
        foreach (var accountId in roleInfo.MemberIds)
        {
            entityPerms = entityPerms with
            {
                AccountEntries = SetPermission(
                    entries: entityPerms.AccountEntries,
                    accountOrRoleId: accountId,
                    sourceId: e.RoleId,
                    permission: e.Permission,
                    grantedAt: metadata.Timestamp
                )
            };
        }
        ops.Store(entityPerms);

        var inheritedPermission = InheritPermission(e.Permission);
        await SpreadRolePermission(ops, e.RoleId, e.EntityId, inheritedPermission, metadata.Timestamp);
    }

    public Task Project(ProjectGroupGlobalPermissionsChanged e, IDocumentOperations ops)
    {
        return SetGlobalPermission(ops, e.ProjectGroupId, e.GlobalPermissions);
    }

    public Task Project(ProjectGlobalPermissionsChanged e, IDocumentOperations ops)
    {
        return SetGlobalPermission(ops, e.ProjectId, e.GlobalPermissions);
    }

    public Task Project(PlaylistGlobalPermissionsChanged e, IDocumentOperations ops)
    {
        return SetGlobalPermission(ops, e.PlaylistId, e.GlobalPermissions);
    }

    public async Task Project(AccountRoleSet e, IEvent metadata, IDocumentOperations ops)
    {
        var query = ops.Query<EntityPermissionInfo>()
            .Where(p => p.RoleEntries.ContainsKey(e.RoleId));
        var affectedEntities = query
            .ToAsyncEnumerable();
        await foreach (var affected in affectedEntities)
        {
            if (!affected.RoleEntries.TryGetValue(e.RoleId, out var roleEntry))
            {
                continue;
            }

            var changed = affected;
            changed = changed with
            {
                AccountEntries = SetPermission(
                    entries: affected.AccountEntries,
                    accountOrRoleId: e.AccountId,
                    sourceId: e.RoleId,
                    permission: affected.RoleEntries[e.RoleId].EffectivePermission,
                    grantedAt: metadata.Timestamp
                )
            };
            ops.Store(changed);
        }
    }

    private static async Task SetGlobalPermission(IDocumentOperations ops, Hrib entityId, Permission globalPermission)
    {
        var entityPerms = await RequireEntityPermissionInfo(ops, entityId);
        entityPerms = entityPerms with
        {
            GlobalPermission = globalPermission & Permission.Publishable
        };
        ops.Store(entityPerms);
    }

    private static async Task<EntityPermissionInfo> RequireEntityPermissionInfo(
        IDocumentOperations ops,
        Hrib entityId)
    {
        var entityPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(entityId);
        if (entityPerms.HasErrors)
        {
            throw new InvalidOperationException($"No permission info exists for '{entityId}'. "
                + "Either the events are out of order or the permission info projection is broken.",
                entityPerms.AsException());
        }
        return entityPerms.Value;
    }

    private static async Task<RoleMembersInfo> RequireRoleMembersInfo(
        IDocumentOperations ops,
        Hrib roleId)
    {
        var roleInfo = await ops.KafeLoadAsync<RoleMembersInfo>(roleId);
        if (roleInfo.HasErrors)
        {
            throw new InvalidOperationException($"No role members info exists for '{roleId}'. "
                + "Either the events are out of order or the permission info projection is broken.",
                roleInfo.AsException());
        }
        return roleInfo.Value;
    }

    private static async Task<EntityPermissionInfo> AddSystem(
        IDocumentOperations ops,
        EntityPermissionInfo perms
    )
    {
        var systemPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(Hrib.System);
        if (systemPerms.HasErrors)
        {
            throw new InvalidOperationException("No permission info exists for the 'system' HRIB. "
                + "The DB is in an invalid state.", systemPerms.AsException());
        }

        return perms with
        {
            GrantorIds = perms.GrantorIds.Add(Hrib.SystemValue),
            AccountEntries = MergeEntries(perms.AccountEntries, InheritEntries(systemPerms.Value.AccountEntries))
        };
    }

    /// <summary>
    /// Adds a parent to the perms object, along with its further ancestors and the `system` object as Grantors.
    /// Also merges in inheritable permission entries from the parent (and thus further ancestors).
    /// </summary>
    private static async Task<EntityPermissionInfo> AddParent(
        IDocumentOperations ops,
        EntityPermissionInfo perms,
        Hrib parentId)
    {
        var ancestorPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(parentId);
        if (ancestorPerms.HasErrors)
        {
            throw new InvalidOperationException($"The parent entity with the '{parentId}' Id does not exist."
                + "Either the Id is wrong or the DB is in an invalid state.", ancestorPerms.AsException());
        }

        return perms with
        {
            AccountEntries = MergeEntries(perms.AccountEntries, InheritEntries(ancestorPerms.Value.AccountEntries)),
            GrantorIds = perms.GrantorIds.Union([parentId.ToString(), .. ancestorPerms.Value.GrantorIds]),
            ParentIds = perms.ParentIds.Add(parentId.ToString())
        };
    }

    /// <summary>
    /// Applies <see cref="InheritPermission"/> to a dictionary of <see cref="EntityPermissionEntry"/>es.
    /// </summary>
    private static ImmutableDictionary<string, EntityPermissionEntry> InheritEntries(
        ImmutableDictionary<string, EntityPermissionEntry> perms
    )
    {
        return perms
            .Where(p => (p.Value.EffectivePermission & Permission.Inheritable) != 0)
            .ToImmutableDictionary(
                p => p.Key,
                p => new EntityPermissionEntry(
                    EffectivePermission: InheritPermission(p.Value.EffectivePermission),
                    Sources: p.Value.Sources
                        .Where(s => (s.Value.Permission & Permission.Inheritable) != 0)
                        .ToImmutableDictionary(s => s.Key, s => s.Value with
                        {
                            Permission = InheritPermission(s.Value.Permission)
                        })
                )
            );
    }

    // NB: This is KAFE's implementation of permission inheritance. Tread carefully. I'm watching you.
    private static Permission InheritPermission(Permission parentPermission)
    {
        return (parentPermission.HasFlag(Permission.Inspect)
                ? Permission.Read
                : Permission.None)
            | (parentPermission & Permission.Inheritable);
    }

    private static EntityPermissionEntry RecalculateEffectivePermission(EntityPermissionEntry entry)
    {
        return entry with
        {
            EffectivePermission = entry.Sources.Values.Aggregate(
                Permission.None,
                (e, s) => e | s.Permission)
        };
    }

    private static ImmutableDictionary<string, EntityPermissionEntry> MergeEntries(
        ImmutableDictionary<string, EntityPermissionEntry> one,
        ImmutableDictionary<string, EntityPermissionEntry> two
    )
    {
        if (one.Count == 0)
        {
            return two;
        }

        if (two.Count == 0)
        {
            return one;
        }

        var result = one.ToBuilder();
        foreach (var pair in two)
        {
            if (result.TryGetValue(pair.Key, out EntityPermissionEntry? existing))
            {
                result[pair.Key] = existing with
                {
                    EffectivePermission = existing.EffectivePermission | pair.Value.EffectivePermission,
                    Sources = existing.Sources.AddRange(pair.Value.Sources)
                };
            }
            else
            {
                result.Add(pair);
            }
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Sets perms for a specific account/role id to a specific entity, in a dictionary of entries.
    /// </summary>
    private static ImmutableDictionary<string, EntityPermissionEntry> SetPermission(
        ImmutableDictionary<string, EntityPermissionEntry> entries,
        Hrib accountOrRoleId,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt)
    {
        if (entries.TryGetValue(accountOrRoleId.ToString(), out var accountEntry))
        {
            // NB: The Sources and EffectivePermission need to be updated separately
            //     so that the event has any effect.
            accountEntry = accountEntry with
            {
                Sources = permission == Permission.None
                    ? accountEntry.Sources.Remove(sourceId.ToString())
                    : accountEntry.Sources.SetItem(sourceId.ToString(), new EntityPermissionSource(
                        Permission: permission,
                        GrantedAt: grantedAt
                    ))
            };
            accountEntry = RecalculateEffectivePermission(accountEntry);
        }
        else if (permission != Permission.None)
        {
            accountEntry = new EntityPermissionEntry(
                EffectivePermission: permission,
                Sources: ImmutableDictionary.CreateRange([new KeyValuePair<string, EntityPermissionSource>(
                    sourceId.ToString(),
                    new EntityPermissionSource(
                        Permission: permission,
                        GrantedAt: grantedAt
                    )
                )])
            );
        }

        if (accountEntry is null)
        {
            return entries;
        }

        return accountEntry.EffectivePermission == Permission.None
                ? entries.Remove(accountOrRoleId.ToString())
                : entries.SetItem(accountOrRoleId.ToString(), accountEntry);
    }

    /// <summary>
    /// Spreads (add, removes or overwries) a set of account permissions to all entities
    /// that have <paramref name="sourceId"/> among its grantors.
    /// </summary>
    private static async Task SpreadAccountPermission(
        IDocumentOperations ops,
        ImmutableHashSet<Hrib> accountIds,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt
    )
    {
        if (accountIds.IsEmpty)
        {
            return;
        }

        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.GrantorIds.Contains(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            var changed = affected;
            foreach (var accountId in accountIds)
            {
                changed = changed with
                {
                    AccountEntries = SetPermission(
                        changed.AccountEntries,
                        accountId,
                        sourceId,
                        permission,
                        grantedAt)
                };
            }
            ops.Store(changed);
        }
    }

    private static async Task SpreadRolePermission(
        IDocumentOperations ops,
        Hrib roleId,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt
    )
    {
        var roleInfo = await RequireRoleMembersInfo(ops, roleId);

        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.GrantorIds.Contains(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            var changed = affected with
            {
                RoleEntries = SetPermission(
                    entries: affected.RoleEntries,
                    accountOrRoleId: roleId,
                    sourceId: sourceId,
                    permission: permission,
                    grantedAt: grantedAt)
            };

            foreach (var accountId in roleInfo.MemberIds)
            {
                changed = changed with
                {
                    AccountEntries = SetPermission(
                        entries: changed.AccountEntries,
                        accountOrRoleId: accountId,
                        sourceId: roleId,
                        permission: changed.RoleEntries[roleId.ToString()].EffectivePermission,
                        grantedAt: grantedAt
                    )
                };
            }
            ops.Store(changed);
        }
    }
}
