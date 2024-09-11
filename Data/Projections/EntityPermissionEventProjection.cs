using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.CodeGeneration.Frames;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Services.Json.Transformations;
using Newtonsoft.Json.Linq;

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

    public async Task<EntityPermissionInfo> Create(RoleCreated e, IDocumentOperations ops)
    {
        var perms = EntityPermissionInfo.Create(e.RoleId);
        perms = await AddSystem(ops, perms);
        return perms;
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

        var entityPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(e.EntityId);
        if (entityPerms.HasErrors)
        {
            throw new InvalidOperationException($"No permission info exists for '{e.EntityId}'. "
                + "Either the events are out of order or the permission info projection is broken.",
                entityPerms.AsException());
        }

        entityPerms = SetAccountPermission(
            perms: entityPerms.Value,
            accountId: e.AccountId,
            // NB: Explicit permissions have source Id equal to the entity Id.
            //     This makes them easy to find and allows for consistent inheriting of info about perms sources.
            sourceId: e.EntityId,
            permission: e.Permission,
            grantedAt: metadata.Timestamp);
        ops.Store(entityPerms.Value);

        var inheritedPermission = InheritPermission(e.Permission);
        // NB: Since we don't know if we're adding or removing permissions we have to try to update all of the entity's
        //     descendants.
        await SpreadAccountPermission(ops, e.AccountId, e.EntityId, inheritedPermission, metadata.Timestamp);
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
            Entries = MergeEntries(perms.Entries, InheritEntries(systemPerms.Value.Entries))
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
            Entries = MergeEntries(perms.Entries, InheritEntries(ancestorPerms.Value.Entries)),
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
    /// Adds perms for a specific account to a specific entity.
    /// </summary>
    private static EntityPermissionInfo SetAccountPermission(
        EntityPermissionInfo perms,
        Hrib accountId,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt)
    {
        if (perms.Entries.TryGetValue(accountId.ToString(), out var accountEntry))
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
            return perms;
        }

        perms = perms with
        {
            Entries = accountEntry.EffectivePermission == Permission.None
                ? perms.Entries.Remove(accountId.ToString())
                : perms.Entries.SetItem(accountId.ToString(), accountEntry)
        };
        return perms;
    }

    /// <summary>
    /// Spreads (add, removes or overwries) an account permission to all entities
    /// that have <paramref name="sourceId"/> among its grantors.
    /// </summary>
    private static async Task SpreadAccountPermission(
        IDocumentOperations ops,
        Hrib accountId,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt)
    {
        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.GrantorIds.Contains(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            var changed = SetAccountPermission(affected, accountId, sourceId, permission, grantedAt);
            ops.Store(changed);
        }
    }
}
