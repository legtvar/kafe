using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
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
        await SpreadAccountPermission(
            ops: ops,
            accountIds: [e.AccountId],
            sourceId: e.EntityId,
            permission: inheritedPermission,
            grantedAt: metadata.Timestamp,
            ignoreSelf: true);
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
                // NB: Explicit permissions have sourceId equal to the entity Id.
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
                    sourceId: e.EntityId,
                    permission: e.Permission,
                    grantedAt: metadata.Timestamp,
                    intermediaryRoleId: e.RoleId
                )
            };
        }
        ops.Store(entityPerms);

        var inheritedPermission = InheritPermission(e.Permission);
        await SpreadRolePermission(
            ops: ops,
            roleId: e.RoleId,
            sourceId: e.EntityId,
            permission: inheritedPermission,
            grantedAt: metadata.Timestamp,
            ignoreSelf: true);
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

    public Task Project(OrganizationGlobalPermissionsChanged e, IDocumentOperations ops)
    {
        return SetGlobalPermission(ops, e.OrganizationId, e.GlobalPermissions);
    }

    public async Task Project(AccountRoleSet e, IEvent metadata, IDocumentOperations ops)
    {
        var membersInfo = await RequireRoleMembersInfo(ops, e.RoleId);
        membersInfo = membersInfo with
        {
            MemberIds = membersInfo.MemberIds.Add(e.AccountId)
        };
        ops.Store(membersInfo);

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
            foreach (var source in roleEntry.Sources)
            {
                changed = changed with
                {
                    AccountEntries = SetPermission(
                        entries: changed.AccountEntries,
                        accountOrRoleId: e.AccountId,
                        sourceId: source.Key,
                        permission: source.Value.Permission,
                        grantedAt: metadata.Timestamp,
                        intermediaryRoleId: e.RoleId
                    )
                };
            }
            ops.Store(changed);
        }
    }

    public async Task Project(AccountRoleUnset e, IDocumentOperations ops)
    {
        var membersInfo = await RequireRoleMembersInfo(ops, e.RoleId);
        membersInfo = membersInfo with
        {
            MemberIds = membersInfo.MemberIds.Remove(e.AccountId)
        };
        ops.Store(membersInfo);

        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.RoleEntries.ContainsKey(e.RoleId))
            .ToAsyncEnumerable();
        await foreach (var affected in affectedEntities)
        {
            if (!affected.RoleEntries.TryGetValue(e.RoleId, out var roleEntry))
            {
                continue;
            }

            if (!affected.AccountEntries.TryGetValue(e.AccountId, out var entry))
            {
                throw new InvalidOperationException("A role-implied permission cannot be removed from an entity "
                    + "because it is not present. This is a bug.");
            }

            entry = entry with
            {
                Sources = entry.Sources.Where(s => s.Value.RoleId != e.RoleId).ToImmutableDictionary()
            };
            entry = RecalculateEffectivePermission(entry);

            var changed = affected;

            changed = entry.EffectivePermission == Permission.None
                ? changed with
                {
                    AccountEntries = changed.AccountEntries.Remove(e.AccountId)
                }
                : changed with
                {
                    AccountEntries = changed.AccountEntries.SetItem(e.AccountId, entry)
                };
            ops.Store(changed);
        }
    }

    public async Task Project(ProjectArtifactAdded e, IDocumentOperations ops)
    {
        var perms = await RequireEntityPermissionInfo(ops, e.ArtifactId);
        perms = await AddParent(ops, perms, e.ProjectId);
        ops.Store(perms);
    }

    public async Task Project(ProjectArtifactRemoved e, IDocumentOperations ops)
    {
        var perms = await RequireEntityPermissionInfo(ops, e.ArtifactId);
        perms = await RemoveDependency(ops, perms, e.ProjectId);
        ops.Store(perms);
    }

    public async Task Project(ProjectGroupMovedToOrganization e, IEvent metadata, IDocumentOperations ops)
    {
        var entitySoFar = await ops.Events.KafeAggregateStream<ProjectGroupInfo>(
            id: e.ProjectGroupId,
            version: metadata.Version - 1);
        if (entitySoFar is null)
        {
            throw new InvalidOperationException("Project group could not be aggregated to a point before the event.");
        }

        var perms = await RequireEntityPermissionInfo(ops, e.ProjectGroupId);
        if (((Hrib)entitySoFar.OrganizationId).IsValidNonEmpty)
        {
            perms = await RemoveDependency(ops, perms, entitySoFar.OrganizationId);
        }

        perms = await AddParent(ops, perms, e.OrganizationId);
        ops.Store(perms);
    }

    public async Task Project(PlaylistMovedToOrganization e, IEvent metadata, IDocumentOperations ops)
    {
        var entitySoFar = await ops.Events.KafeAggregateStream<ProjectGroupInfo>(
            id: e.PlaylistId,
            version: metadata.Version - 1);
        if (entitySoFar is null)
        {
            throw new InvalidOperationException("Project group could not be aggregated to a point before the event.");
        }

        var perms = await RequireEntityPermissionInfo(ops, e.PlaylistId);
        if (((Hrib)entitySoFar.OrganizationId).IsValidNonEmpty)
        {
            perms = await RemoveDependency(ops, perms, entitySoFar.OrganizationId);
        }

        perms = await AddParent(ops, perms, e.OrganizationId);
        ops.Store(perms);
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

    /// <summary>
    /// Add Hrib.System to <see cref="EntityPermissionInfo.DependencyGraph"/>.
    /// Also, adds account entries for all accounts that have permissions for System with inherited permissions values.
    /// </summary>
    private static async Task<EntityPermissionInfo> AddSystem(
        IDocumentOperations ops,
        EntityPermissionInfo perms
    )
    {
        var systemPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(Hrib.System);
        if (systemPerms.HasErrors && systemPerms.Errors.Length == 1 && systemPerms.Errors[0].Id == Error.NotFoundId)
        {
            systemPerms = EntityPermissionInfo.Create(Hrib.SystemValue);
            ops.Store(systemPerms.Value);
        }

        return perms with
        {
            AccountEntries = MergeEntries(perms.AccountEntries, InheritEntries(systemPerms.Value.AccountEntries)),
            DependencyGraph = perms.DependencyGraph.SetItem(Hrib.SystemValue, [Hrib.SystemValue])
        };
    }

    /// <summary>
    /// Adds a parent to the perms's <see cref="EntityPermissionInfo.DependencyGraph"/>,
    /// along with its further ancestors and the `system` object.
    /// Also merges in inheritable permission entries from the parent (and thus further ancestors).
    /// </summary>
    private static async Task<EntityPermissionInfo> AddParent(
        IDocumentOperations ops,
        EntityPermissionInfo perms,
        Hrib parentId
    )
    {
        var ancestorPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(parentId);
        if (ancestorPerms.HasErrors)
        {
            throw new InvalidOperationException($"The parent entity with the '{parentId}' Id does not exist."
                + "Either the Id is wrong or the DB is in an invalid state.", ancestorPerms.AsException());
        }

        var changedPerms = perms with
        {
            AccountEntries = MergeEntries(perms.AccountEntries, InheritEntries(ancestorPerms.Value.AccountEntries)),
            RoleEntries = MergeEntries(perms.RoleEntries, InheritEntries(ancestorPerms.Value.RoleEntries)),
            DependencyGraph = MergeDependencyGraphs(perms.DependencyGraph, ancestorPerms.Value.DependencyGraph)
                .SetItem(ancestorPerms.Value.Id, [perms.Id])
        };

        // NB: "Spread" the new dependency to all entities where `perms.Id` was already in the dependency graph
        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.Id != changedPerms.Id && p.DependencyGraph.ContainsKey(changedPerms.Id))
            .ToAsyncEnumerable();
        await foreach (var affected in affectedEntities)
        {
            var changed = affected with
            {
                AccountEntries = MergeEntries(affected.AccountEntries, InheritEntries(changedPerms.AccountEntries)),
                RoleEntries = MergeEntries(affected.RoleEntries, InheritEntries(changedPerms.RoleEntries)),
                DependencyGraph = MergeDependencyGraphs(affected.DependencyGraph, changedPerms.DependencyGraph)
            };
            ops.Store(changed);
        }

        ops.Store(changedPerms);
        return changedPerms;
    }

    /// <summary>
    /// Removes a dependency from an <see cref="EntityPermissionInfo.DependencyGraph"/> along with its transitive
    /// dependencies (but only should the become orphans) and related account and role entries.
    /// </summary>
    private static async Task<EntityPermissionInfo> RemoveDependency(
        IDocumentOperations ops,
        EntityPermissionInfo perms,
        Hrib dependencyId
    )
    {
        var changedGraph = RemoveFromDependencyGraph(
            perms.DependencyGraph,
            dependencyId.ToString(),
            out var removedGrantors);

        var changedPerms = perms with
        {
            AccountEntries = RemoveEntrySources(perms.AccountEntries, removedGrantors),
            RoleEntries = RemoveEntrySources(perms.RoleEntries, removedGrantors),
            DependencyGraph = changedGraph
        };

        // NB: Recalculate perms of entities where `perms` is a grantor.
        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.Id != changedPerms.Id && p.DependencyGraph.ContainsKey(changedPerms.Id))
            .ToAsyncEnumerable();
        await foreach (var affected in affectedEntities)
        {
            var affectedGraph = RemoveFromDependencyGraph(
                affected.DependencyGraph,
                dependencyId.ToString(),
                out var removedAffectedGrantors,
                changedPerms.Id);

            var changed = affected with
            {
                DependencyGraph = affectedGraph,
                AccountEntries = RemoveEntrySources(affected.AccountEntries, removedAffectedGrantors),
                RoleEntries = RemoveEntrySources(affected.RoleEntries, removedAffectedGrantors)
            };
            ops.Store(changed);
        }

        ops.Store(changedPerms);
        return changedPerms;
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

    private static ImmutableDictionary<string, EntityPermissionEntry> RemoveEntrySources(
        ImmutableDictionary<string, EntityPermissionEntry> entries,
        ImmutableHashSet<string> sources
    )
    {
        if (sources.IsEmpty)
        {
            return entries;
        }

        var result = entries.ToBuilder();
        foreach (var pair in entries)
        {
            var newSources = pair.Value.Sources.ExceptBy(sources, s => s.Key).ToImmutableDictionary();
            if (newSources.Count < pair.Value.Sources.Count)
            {
                result[pair.Key] = RecalculateEffectivePermission(pair.Value with
                {
                    Sources = newSources
                });

                if (result[pair.Key].EffectivePermission == Permission.None)
                {
                    result.Remove(pair.Key);
                }
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
        DateTimeOffset grantedAt,
        string? intermediaryRoleId = null)
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
                        GrantedAt: grantedAt,
                        RoleId: intermediaryRoleId
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
                        GrantedAt: grantedAt,
                        RoleId: intermediaryRoleId
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
        DateTimeOffset grantedAt,
        bool ignoreSelf = false
    )
    {
        if (accountIds.IsEmpty)
        {
            return;
        }

        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.DependencyGraph.ContainsKey(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            if (affected.Id == sourceId && ignoreSelf)
            {
                continue;
            }

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
        DateTimeOffset grantedAt,
        bool ignoreSelf = false
    )
    {
        var roleInfo = await RequireRoleMembersInfo(ops, roleId);

        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.DependencyGraph.ContainsKey(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            if (affected.Id == sourceId && ignoreSelf)
            {
                continue;
            }

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
                        sourceId: sourceId,
                        permission: permission,
                        grantedAt: grantedAt,
                        intermediaryRoleId: roleId.ToString()
                    )
                };
            }
            ops.Store(changed);
        }
    }

    private static ImmutableDictionary<string, ImmutableHashSet<string>> MergeDependencyGraphs(
        ImmutableDictionary<string, ImmutableHashSet<string>> one,
        ImmutableDictionary<string, ImmutableHashSet<string>> two
    )
    {
        var builder = one.ToBuilder();
        foreach (var pair in two)
        {
            if (!builder.TryAdd(pair.Key, pair.Value))
            {
                builder[pair.Key] = pair.Value.Union(builder[pair.Key]);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, ImmutableHashSet<string>> RemoveFromDependencyGraph(
        ImmutableDictionary<string, ImmutableHashSet<string>> graph,
        string removedId,
        out ImmutableHashSet<string> removedKeys,
        string? sourceId = null
    )
    {
        var builder = graph.ToBuilder();

        if (sourceId != null)
        {
            builder[removedId] = builder[removedId].Remove(sourceId);
            // NB: special case: `sourceId` was not the only source of the `removedId` dependency
            //                   thus we cannot remove `removedId`
            if (builder[removedId].Count > 0)
            {
                removedKeys = [];
                return builder.ToImmutable();
            }
        }

        var removedBuilder = ImmutableHashSet.CreateBuilder<string>();
        var queue = new Queue<string>();
        queue.Enqueue(removedId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            builder.Remove(current);
            removedBuilder.Add(current);
            var ancestors = builder.Where(p => p.Value.Contains(current))
                .Select(p => p.Key)
                .ToArray();
            foreach (var ancestor in ancestors)
            {
                builder[ancestor] = builder[ancestor].Remove(current);
                if (builder[ancestor].IsEmpty)
                {
                    queue.Enqueue(ancestor);
                }
            }
        }
        removedKeys = removedBuilder.ToImmutable();
        return builder.ToImmutable();
    }
}
