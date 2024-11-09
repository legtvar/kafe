using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Aggregates;

public record RoleInfo(
    [Hrib] string Id,

    CreationMethod CreationMethod,

    [Hrib] string OrganizationId,

    [property:LocalizedString]
    [property:Sortable]
    ImmutableDictionary<string, string> Name,

    [property:LocalizedString]
    [property:Sortable]
    ImmutableDictionary<string, string>? Description,

    [property:Sortable]
    DateTimeOffset CreatedOn,

    ImmutableDictionary<string, Permission> Permissions
) : IEntity
{
    public static readonly RoleInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        OrganizationId: Hrib.InvalidValue,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        Description: null,
        CreatedOn: default,
        Permissions: ImmutableDictionary<string, Permission>.Empty
    );

    public RoleInfo() : this(Invalid)
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="RoleInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static RoleInfo Create(Hrib organizationId, LocalizedString name)
    {
        return new RoleInfo
        {
            Id = Hrib.EmptyValue,
            OrganizationId = organizationId.RawValue,
            Name = name
        };
    }
}

public class RoleInfoProjection : SingleStreamProjection<RoleInfo>
{
    public RoleInfoProjection()
    {
    }

    public static RoleInfo Create(IEvent<RoleCreated> e)
    {
        return new RoleInfo(
            Id: e.Data.RoleId,
            CreationMethod: e.Data.CreationMethod,
            OrganizationId: e.Data.OrganizationId,
            Name: e.Data.Name,
            Description: null,
            CreatedOn: e.Timestamp,
            Permissions: ImmutableDictionary<string, Permission>.Empty);
    }

    public RoleInfo Apply(RoleInfoChanged e, RoleInfo r)
    {
        return r with
        {
            Name = e.Name ?? r.Name,
            Description = e.Description ?? r.Description
        };
    }

    public RoleInfo Apply(RolePermissionSet e, RoleInfo r)
    {
        if (r.Permissions is null)
        {
            r = r with { Permissions = ImmutableDictionary<string, Permission>.Empty };
        }

        return r with
        {
            Permissions = e.Permission == Permission.None
                ? r.Permissions.Remove(e.EntityId)
                : r.Permissions.SetItem(e.EntityId, e.Permission)
        };
    }
}
