using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Marten;
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

    private static async Task<EntityPermissionInfo> AddSystem(
        IDocumentOperations ops,
        EntityPermissionInfo perms
    )
    {
        var systemPerms = await ops.LoadAsync<EntityPermissionInfo>(Hrib.System)
            ?? throw new InvalidOperationException("No permission info exists for the 'system' HRIB. "
                + "The DB is in an invalid state.");
        return perms with
        {
            GrantorIds = perms.GrantorIds.Add(Hrib.SystemValue),
            Entries = MergeEntries(perms.Entries, systemPerms.Entries)
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
        var ancestorPerms = await ops.LoadAsync<EntityPermissionInfo>(parentId)
            ?? throw new InvalidOperationException($"The parent entity with the '{parentId}' Id does not exist."
                + "Either the Id is wrong or the DB is in an invalid state.");

        return perms with
        {
            Entries = MergeEntries(perms.Entries, InheritPermissions(ancestorPerms.Entries)),
            GrantorIds = perms.GrantorIds.Union([parentId.ToString(), .. ancestorPerms.GrantorIds]),
            ParentIds = perms.ParentIds.Add(parentId.ToString())
        };
    }

    private static ImmutableDictionary<string, EntityPermissionEntry> InheritPermissions(
        ImmutableDictionary<string, EntityPermissionEntry> perms
    )
    {
        return perms
            .Where(p => (p.Value.EffectivePermission & Permission.Inheritable) != 0)
            .ToImmutableDictionary(
                p => p.Key,
                p => new EntityPermissionEntry(
                    EffectivePermission: (p.Value.EffectivePermission.HasFlag(Permission.Inspect)
                            ? Permission.Read
                            : Permission.None)
                        | (p.Value.EffectivePermission & Permission.Inheritable),
                    Sources: p.Value.Sources
                )
            );
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
}
