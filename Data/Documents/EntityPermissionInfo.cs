using System;
using System.Collections.Immutable;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Documents;

public record PermissionSource(
    Permission Permission,
    [Hrib] ImmutableArray<string> SourceIds,
    DateTimeOffset GivenAt
);

public record EntityPermissionInfo(
    [Hrib] string Id,
    ImmutableHashSet<string> ParentEntityIds,
    ImmutableDictionary<string, PermissionSource> Accounts,
    ImmutableDictionary<string, PermissionSource> Roles
)
{
    public static readonly EntityPermissionInfo Invalid = new(
        Id: Hrib.InvalidValue,
        ParentEntityIds: [],
        Accounts: ImmutableDictionary<string, PermissionSource>.Empty,
        Roles: ImmutableDictionary<string, PermissionSource>.Empty
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
