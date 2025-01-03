using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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

    public Task Project(ProjectGroupGlobalPermissionsChanged e, IEvent metadata, IDocumentOperations ops)
    {
        return OnGlobalPermissionChanged(ops, e.ProjectGroupId, e.GlobalPermissions, metadata.Timestamp);
    }

    public Task Project(ProjectGlobalPermissionsChanged e, IEvent metadata, IDocumentOperations ops)
    {
        return OnGlobalPermissionChanged(ops, e.ProjectId, e.GlobalPermissions, metadata.Timestamp);
    }

    public Task Project(PlaylistGlobalPermissionsChanged e, IEvent metadata, IDocumentOperations ops)
    {
        return OnGlobalPermissionChanged(ops, e.PlaylistId, e.GlobalPermissions, metadata.Timestamp);
    }

    public Task Project(OrganizationGlobalPermissionsChanged e, IEvent metadata, IDocumentOperations ops)
    {
        return OnGlobalPermissionChanged(ops, e.OrganizationId, e.GlobalPermissions, metadata.Timestamp);
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

    private static async Task OnGlobalPermissionChanged(
        IDocumentOperations ops,
        Hrib entityId,
        Permission globalPermission,
        DateTimeOffset grantedAt
    )
    {
        var entityPerms = await RequireEntityPermissionInfo(ops, entityId);
        entityPerms = SetGlobalPermission(entityPerms, entityId, globalPermission, grantedAt);
        ops.Store(entityPerms);

        await SpreadGlobalPermission(
            ops: ops,
            sourceId: entityId,
            permission: InheritPermission(globalPermission),
            grantedAt: grantedAt,
            ignoreSelf: true
        );
    }

    private static EntityPermissionInfo SetGlobalPermission(
        EntityPermissionInfo entityPerms,
        Hrib sourceId,
        Permission globalPermission,
        DateTimeOffset grantedAt)
    {
        globalPermission &= Permission.Publishable;

        var entry = entityPerms.GlobalPermission;
        entry = entry with
        {
            Sources = globalPermission == Permission.None
                ? entry.Sources.Remove(sourceId.ToString())
                : entry.Sources.SetItem(sourceId.ToString(), new EntityPermissionSource(
                    Permission: globalPermission,
                    GrantedAt: grantedAt,
                    RoleId: null
                ))
        };
        entry = RecalculateEffectivePermission(entry);
        entityPerms = entityPerms with { GlobalPermission = entry };
        return entityPerms;
    }

    private static async Task<EntityPermissionInfo> RequireEntityPermissionInfo(
        IDocumentOperations ops,
        Hrib entityId)
    {
        var entityPerms = await ops.KafeLoadAsync<EntityPermissionInfo>(entityId);
        if (entityPerms.HasErrors)
        {
            // NB: Once upon a time, there were Capabilities instead of Permissions.
            //     They were terribly designed and served the only the first year KAFE was used in production.
            //     When they were deprecated, a heroic upcaster was posted betweeen KAFE and the DB so that we'd never
            //     have to think about them again... or so we though...
            //
            //     Unfortunately, the FFFIMU23 project group came to exist AFTER some of the AccountCapabilityAdded
            //     events were already issued. More specifily events 10909, 10911, 10912, 10916, and 10917 are the band
            //     of misfits that somehow got created before 10918, when the group itself came to be.
            //    
            //     So now here we are... and those five events are bombs that destroy this projection every time they
            //     are encountered because they reference a group that will not have existed for another less than a
            //     second. Thus we have a new hero, this `if` below that will hopefully save us from the mistake
            //     which calls itself Capabilities.
            if (entityId == Kafe.Data.Events.Upcasts.AccountCapabilityAddedUpcaster.Fffimu23Id)
            {
                // It doesn't matter what we return. It will get ovewritten a few events later, when the project group
                // actually gets created.
                return EntityPermissionInfo.Create(entityId);
            }

            // NB: Twice upon a time... I was dumb... and made a mistake. This time in the LegacyOrganizationCorrection,
            //     where I first appended the ProjectGroupMovedToOrganization and PlaylistMovedToOrganization events
            //     and only THEN created the legacy--org organization. Thus, we have another of prescience to deal with,
            //     because those few events KNOW that an organization shall exist and indeed it does.
            if (entityId == Kafe.Data.Events.Corrections.LegacyOrganizationCorrection.LegacyOrganizationId)
            {
                return EntityPermissionInfo.Create(entityId);
            }

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
            AccountEntries = MergeManyEntries(
                perms.AccountEntries,
                InheritEntries(systemPerms.Value.AccountEntries)),
            // NB: We ignore RoleEntries and GlobalPermission, since `system` can have neither.
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
        var ancestorPerms = await RequireEntityPermissionInfo(ops, parentId);

        var changedPerms = perms with
        {
            AccountEntries = MergeManyEntries(
                perms.AccountEntries,
                InheritEntries(ancestorPerms.AccountEntries)),
            RoleEntries = MergeManyEntries(
                perms.RoleEntries,
                InheritEntries(ancestorPerms.RoleEntries)),
            GlobalPermission = MergeTwoEntries(
                perms.GlobalPermission,
                InheritEntry(ancestorPerms.GlobalPermission)
            ),
            DependencyGraph = MergeDependencyGraphs(perms.DependencyGraph, ancestorPerms.DependencyGraph)
                .SetItem(ancestorPerms.Id, [perms.Id])
        };

        // NB: "Spread" the new dependency to all entities where `perms.Id` was already in the dependency graph
        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.Id != changedPerms.Id && p.DependencyGraph.ContainsKey(changedPerms.Id))
            .ToAsyncEnumerable();
        await foreach (var affected in affectedEntities)
        {
            var changed = affected with
            {
                AccountEntries = MergeManyEntries(
                    affected.AccountEntries,
                    InheritEntries(changedPerms.AccountEntries)),
                RoleEntries = MergeManyEntries(
                    affected.RoleEntries,
                    InheritEntries(changedPerms.RoleEntries)),
                GlobalPermission = MergeTwoEntries(
                    affected.GlobalPermission,
                    InheritEntry(changedPerms.GlobalPermission)
                ),
                DependencyGraph = MergeDependencyGraphs(affected.DependencyGraph, changedPerms.DependencyGraph)
            };
            ops.Store(changed);
        }

        ops.Store(changedPerms);
        return changedPerms;
    }

    /// <summary>
    /// Removes a dependency from an <see cref="EntityPermissionInfo.DependencyGraph"/> along with its transitive
    /// dependencies (but only should they become orphans) and related account and role entries.
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
            GlobalPermission = RemoveEntrySources(perms.GlobalPermission, removedGrantors),
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
                RoleEntries = RemoveEntrySources(affected.RoleEntries, removedAffectedGrantors),
                GlobalPermission = RemoveEntrySources(affected.GlobalPermission, removedAffectedGrantors)
            };
            ops.Store(changed);
        }

        ops.Store(changedPerms);
        return changedPerms;
    }

    private static EntityPermissionEntry InheritEntry(EntityPermissionEntry entry)
    {
        return new EntityPermissionEntry(
            EffectivePermission: InheritPermission(entry.EffectivePermission),
            Sources: entry.Sources
                .Where(s => (s.Value.Permission & Permission.Inheritable) != 0)
                .ToImmutableDictionary(s => s.Key, s => s.Value with
                {
                    Permission = InheritPermission(s.Value.Permission)
                })
        );
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
                p => InheritEntry(p.Value)
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

    private static EntityPermissionEntry MergeTwoEntries(
        EntityPermissionEntry one,
        EntityPermissionEntry two
    )
    {
        return one with
        {
            EffectivePermission = one.EffectivePermission | two.EffectivePermission,
            Sources = one.Sources.AddRange(two.Sources)
        };
    }

    private static ImmutableDictionary<string, EntityPermissionEntry> MergeManyEntries(
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
                result[pair.Key] = MergeTwoEntries(existing, pair.Value);
            }
            else
            {
                result.Add(pair);
            }
        }

        return result.ToImmutable();
    }

    private static EntityPermissionEntry RemoveEntrySources(
        EntityPermissionEntry entry,
        ImmutableHashSet<string> sources
    )
    {
        if (sources.IsEmpty)
        {
            return entry;
        }

        var newSources = entry.Sources.ExceptBy(sources, s => s.Key).ToImmutableDictionary();
        if (newSources.Count >= entry.Sources.Count)
        {
            return entry;
        }

        var newEntry = RecalculateEffectivePermission(entry with
        {
            Sources = newSources
        });

        return newEntry;
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
            var newEntry = RemoveEntrySources(pair.Value, sources);
            if (newEntry.EffectivePermission != Permission.None)
            {
                result[pair.Key] = newEntry;
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
        if (entries.TryGetValue(accountOrRoleId.ToString(), out var entry))
        {
            // NB: The Sources and EffectivePermission need to be updated separately
            //     so that the event has any effect.
            entry = entry with
            {
                Sources = permission == Permission.None
                    ? entry.Sources.Remove(sourceId.ToString())
                    : entry.Sources.SetItem(sourceId.ToString(), new EntityPermissionSource(
                        Permission: permission,
                        GrantedAt: grantedAt,
                        RoleId: intermediaryRoleId
                    ))
            };
            entry = RecalculateEffectivePermission(entry);
        }
        else if (permission != Permission.None)
        {
            entry = new EntityPermissionEntry(
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

        if (entry is null)
        {
            return entries;
        }

        return entry.EffectivePermission == Permission.None
                ? entries.Remove(accountOrRoleId.ToString())
                : entries.SetItem(accountOrRoleId.ToString(), entry);
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

    private static async Task SpreadGlobalPermission(
        IDocumentOperations ops,
        Hrib sourceId,
        Permission permission,
        DateTimeOffset grantedAt,
        bool ignoreSelf = false
    )
    {
        var affectedEntities = ops.Query<EntityPermissionInfo>()
            .Where(p => p.DependencyGraph.ContainsKey(sourceId.ToString()))
            .ToAsyncEnumerable();

        await foreach (var affected in affectedEntities)
        {
            if (affected.Id == sourceId && ignoreSelf)
            {
                continue;
            }

            var changed = SetGlobalPermission(affected, sourceId, permission, grantedAt);
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
