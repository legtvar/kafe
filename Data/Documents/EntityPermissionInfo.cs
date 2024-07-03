using System;
using System.Collections.Immutable;
using Marten.Events.CodeGeneration;
using Kafe.Data.Aggregates;

namespace Kafe.Data.Documents;

public record EntityPermissionSource(
    Permission Permission,
    DateTimeOffset GrantedAt
);

public record EntityPermissionItem(
    Permission EffectivePermission,
    ImmutableDictionary<string, EntityPermissionSource> Sources
);

/// <summary>
/// Describes all accounts that can have any permissions (other than <see cref="IVisibleEntity.GlobalPermissions"/>).
/// </summary>
/// <param name="Id">The Id of the entity---the object the permissions affect.</param>
/// <param name="GrantorIds">
/// Ids of all transitive parent entities whose permission affect this entity.
/// For example, if this entity is a project, <see cref="GrantorIds"/> will contain the Ids of the
/// containing project group.
/// In other words, contains all entities, whose change, may affect this <see cref="EntityPermissionInfo"/>.
/// </param>
/// <param name="ParentIds">Ids of all direct parent entities whose permission affect this entity.</param>
/// <param name="Accounts">The list of accounts with permissions to this entity along with source metadata.</param>
public record EntityPermissionInfo(
    [Hrib] string Id,
    ImmutableHashSet<string> GrantorIds,
    ImmutableHashSet<string> ParentIds,
    ImmutableDictionary<string, EntityPermissionItem> Accounts
)
{
    public static readonly EntityPermissionInfo Invalid = new(
        Id: Hrib.InvalidValue,
        GrantorIds: [],
        ParentIds: [],
        Accounts: ImmutableDictionary<string, EntityPermissionItem>.Empty
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
}
