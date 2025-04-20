using System;
using System.Collections.Immutable;
using Marten.Events.CodeGeneration;
using Kafe.Data.Aggregates;
using System.Linq;

namespace Kafe.Data.Documents;

/// <summary>
/// A source of a permissions. Used to calculate effective permissions, and to add and remove permissions correctly.
/// </summary>
/// <param name="Permission">The permission being granted by the source.</param>
/// <param name="GrantedAt">The timestamp from which this source grants the permission.</param>
/// <param name="RoleId">
/// The Id of the Role that acted as an intermediary for the account to obtain permission from the source.
/// </param>
public record EntityPermissionSource(
    Permission Permission,
    DateTimeOffset GrantedAt,
    string? RoleId
);

/// <summary>
/// Describes permissions of an Account/Role to an entity.
/// </summary>
/// <param name="EffectivePermission">The real, usuable permissions to be applied.</param>
/// <param name="Sources">The sources and original permissions applied.</param>
public record EntityPermissionEntry(
    Permission EffectivePermission,
    ImmutableDictionary<string, EntityPermissionSource> Sources
);

/// <summary>
/// Describes all accounts that can have any permissions (other than <see cref="IVisibleEntity.GlobalPermissions"/>).
/// </summary>
/// 
/// <param name="Id">The Id of the entity---the object the permissions affect.</param>
/// <param name="DependencyGraph">
/// Keys are all "grantors" -- entities whose permission influence permissionf of *this* entity.
/// Values are sources of those grantors.
/// Keys the value of which contains <paramref name="Id"/> are "parents.
/// The sources are important when adding or removing parents of entities.
/// Always contains [<paramref name="Id"/>] = {<paramref name="Id"/>} and [<see cref="Hrib.System"/>] = {<see cref="Hrib.System"/>}.
/// </param>

/// <param name="AccountEntries"> Accounts with permissions to this entity along with source metadata.</param>
/// <param name="RoleEntries"> Roles with permissions to this entity along with source metadata.</param>
public record EntityPermissionInfo(
    [Hrib] string Id,
    EntityPermissionEntry GlobalPermission,
    ImmutableDictionary<string, ImmutableHashSet<string>> DependencyGraph,
    ImmutableDictionary<string, EntityPermissionEntry> RoleEntries,
    ImmutableDictionary<string, EntityPermissionEntry> AccountEntries
) : IEntity
{
    public static readonly EntityPermissionInfo Invalid = new();

    Hrib IEntity.Id => Id;

    public EntityPermissionInfo() : this(
        Id: Hrib.InvalidValue,
        GlobalPermission: new(Permission.None, ImmutableDictionary<string, EntityPermissionSource>.Empty),
        DependencyGraph: ImmutableDictionary<string, ImmutableHashSet<string>>.Empty,
        RoleEntries: ImmutableDictionary<string, EntityPermissionEntry>.Empty,
        AccountEntries: ImmutableDictionary<string, EntityPermissionEntry>.Empty
    )
    {}

    /// <summary>
    /// Creates a bare-bones but valid <see cref="EntityPermissionInfo"/> with nothing but the entity's HRIB.
    /// </summary>
    [MartenIgnore]
    public static EntityPermissionInfo Create(Hrib id)
    {
        var graph = ImmutableDictionary.CreateBuilder<string, ImmutableHashSet<string>>();
        graph.Add(id.ToString(), [id.ToString()]);
        return new EntityPermissionInfo() with
        {
            Id = id.ToString(),
            DependencyGraph = graph.ToImmutable()
        };
    }

    public Permission GetAccountPermission(Hrib accountId)
    {
        return (AccountEntries.GetValueOrDefault(accountId.ToString())?.EffectivePermission ?? Permission.None)
            | GlobalPermission.EffectivePermission;
    }

    public ImmutableHashSet<string> GetParents()
    {
        return DependencyGraph.Where(p => p.Key != Id && p.Value.Contains(Id))
            .Select(p => p.Key)
            .ToImmutableHashSet();
    }

    public ImmutableHashSet<string> GetGrantors()
    {
        return [.. DependencyGraph.Keys];
    }
}
