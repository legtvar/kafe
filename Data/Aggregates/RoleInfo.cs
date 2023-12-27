using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record RoleInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string OrganizationId,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description,
    DateTimeOffset CreatedOn,
    ImmutableDictionary<string, Permission> Permissions
) : IEntity
{
    public static readonly RoleInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        OrganizationId: Hrib.InvalidValue,
        Name: null!,
        Description: null,
        CreatedOn: default,
        Permissions: null!
    );
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
            Permissions = r.Permissions.SetItem(e.EntityId, e.Permission)
        };
    }

    public RoleInfo Apply(RolePermissionUnset e, RoleInfo r)
    {
        if (r.Permissions is null)
        {
            r = r with { Permissions = ImmutableDictionary<string, Permission>.Empty };
        }

        return r with
        {
            Permissions = r.Permissions.Remove(e.EntityId)
        };
    }
}
