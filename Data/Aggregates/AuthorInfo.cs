using Marten.Events.Aggregation;
using Kafe.Data.Events;
using Marten.Events;

namespace Kafe.Data.Aggregates;

public record AuthorInfo(
    string Id,
    CreationMethod CreationMethod,
    string Name,
    LocalizedString? Bio = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null) : IEntity;

public class AuthorInfoProjection : SingleStreamAggregation<AuthorInfo>
{
    public AuthorInfoProjection()
    {
    }

    public AuthorInfo Create(AuthorCreated e)
    {
        return new AuthorInfo(
            Id: e.AuthorId,
            CreationMethod: e.CreationMethod,
            Name: e.Name);
    }

    public AuthorInfo Apply(AuthorInfoChanged e, AuthorInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            Bio = e.Bio ?? a.Bio,
            Uco = e.Uco ?? a.Uco,
            Email = e.Email ?? a.Email,
            Phone = e.Phone ?? a.Phone
        };
    }
}
