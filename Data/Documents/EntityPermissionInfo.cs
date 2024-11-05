using System;
using System.Collections.Immutable;
using Marten.Events.CodeGeneration;
using Kafe.Data.Aggregates;

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
/// 
/// <param name="GrantorIds">
/// Ids of all transitive parent entities whose permission affect this entity.
/// For example, if this entity is a project, <see cref="GrantorIds"/> will contain the Ids of the
/// containing project group.
/// In other words, contains all entities, whose change, may affect this <see cref="EntityPermissionInfo"/>.
/// Please NOTE that Roles CANNOT be grantors.
/// If they were, we woulnd't be able to correctly recalculate permissions when an entity moves or changes parent
/// (e.g., <see cref="Events.ProjectArtifactRemoved"/>).
/// </param>
/// 
/// <param name="ParentIds">Ids of all direct parent entities whose permission affect this entity.</param>
/// 
/// <param name="AccountEntries"> Accounts with permissions to this entity along with source metadata.</param>
/// <param name="RoleEntries"> Roles with permissions to this entity along with source metadata.</param>
public record EntityPermissionInfo(
    [Hrib] string Id,
    Permission GlobalPermission,
    ImmutableHashSet<string> GrantorIds,
    ImmutableHashSet<string> ParentIds,
    ImmutableDictionary<string, EntityPermissionEntry> RoleEntries,
    ImmutableDictionary<string, EntityPermissionEntry> AccountEntries
) : IEntity
{
    public static readonly EntityPermissionInfo Invalid = new(
        Id: Hrib.InvalidValue,
        GlobalPermission: Permission.None,
        GrantorIds: [],
        ParentIds: [],
        RoleEntries: ImmutableDictionary<string, EntityPermissionEntry>.Empty,
        AccountEntries: ImmutableDictionary<string, EntityPermissionEntry>.Empty
    );

    public EntityPermissionInfo() : this(Invalid)
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="EntityPermissionInfo"/> with nothing but the entity's HRIB.
    /// </summary>
    [MartenIgnore]
    public static EntityPermissionInfo Create(Hrib id)
    {
        return new EntityPermissionInfo() with { Id = id.ToString() };
    }

    public Permission GetAccountPermission(Hrib accountId)
    {
        return (AccountEntries.GetValueOrDefault(accountId.ToString())?.EffectivePermission ?? Permission.None)
            | GlobalPermission;
    }
}
