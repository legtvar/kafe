using System.Collections.Immutable;
using Kafe.Data.Aggregates;
using Kafe.Data.Projections;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Documents;

/// <summary>
/// Info about members of a <see cref="RoleInfo"/>.
/// </summary>
/// <remarks>
/// This is a supplementary document for <see cref="EntityPermissionInfo"/>.
/// It could've been a multistream aggregate but it is instead managed by <see cref="EntityPermissionEventProjection"/>
/// to avoid race conditions in the projection cause by Marten's parallel processing of projections.
/// </remarks>
public record RoleMembersInfo
(
    [Hrib] string Id,
    ImmutableHashSet<string> MemberIds
) : IEntity
{
    public static readonly RoleMembersInfo Invalid = new();

    public RoleMembersInfo() : this(
        Id: Hrib.InvalidValue,
        MemberIds: []
    )
    {
    }

    Hrib IEntity.Id => Id;

    /// <summary>
    /// Creates a bare-bones but valid <see cref="RoleMembersInfo"/> with nothing but the entity's HRIB.
    /// </summary>
    [MartenIgnore]
    public static RoleMembersInfo Create(Hrib id)
    {
        return new RoleMembersInfo() with { Id = id.ToString() };
    }
}
