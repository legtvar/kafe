using Marten.Events.Aggregation;
using Kafe.Data.Events;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record AuthorInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    string Name,
    Permission GlobalPermissions = Permission.None,
    [LocalizedString] ImmutableDictionary<string, string>? Bio = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null) : IVisibleEntity
{
    public AuthorInfo() : this(Hrib.InvalidValue, CreationMethod.Unknown, Const.InvalidName)
    {
    }
}

public class AuthorInfoProjection : SingleStreamProjection<AuthorInfo>
{
    public AuthorInfoProjection()
    {
    }

    public static AuthorInfo Create(AuthorCreated e)
    {
        return new AuthorInfo(
            Id: e.AuthorId,
            CreationMethod: e.CreationMethod,
            Name: e.Name); ;
    }

    public AuthorInfo Apply(AuthorInfoChanged e, AuthorInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            GlobalPermissions = e.GlobalPermissions ?? a.GlobalPermissions,
            Bio = e.Bio ?? a.Bio,
            Uco = e.Uco ?? a.Uco,
            Email = e.Email ?? a.Email,
            Phone = e.Phone ?? a.Phone
        };
    }

    public AuthorInfo Apply(AuthorGlobalPermissionsChanged e, AuthorInfo a)
    {
        return a with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }
}
