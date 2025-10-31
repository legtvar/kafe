using Marten.Events.Aggregation;
using Kafe.Data.Events;
using System.Collections.Immutable;
using Npgsql;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Aggregates;

public record AuthorInfo(
    [Hrib] string Id,

    CreationMethod CreationMethod,

    [property:Sortable]
    string Name,

    Permission GlobalPermissions = Permission.None,

    [LocalizedString] ImmutableDictionary<string, string>? Bio = null,

    [property:Sortable]
    string? Uco = null,

    [property:Sortable]
    string? Email = null,

    [property:Sortable]
    string? Phone = null) : IVisibleEntity
{
    public static readonly AuthorInfo Invalid = new();

    public AuthorInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Name: Const.InvalidName,
        GlobalPermissions: Permission.None,
        Bio: null,
        Uco: null,
        Email: null,
        Phone: null
    )
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="AuthorInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static AuthorInfo Create(string name)
    {
        return new AuthorInfo() with
        {
            Id = Hrib.EmptyValue,
            Name = name
        };
    }
}

public class AuthorInfoProjection : SingleStreamProjection<AuthorInfo, string>
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
