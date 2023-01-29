using Marten.Events.Aggregation;
using Kafe.Data.Events;
using Marten.Events;

namespace Kafe.Data.Aggregates;

public record Author(
    string Id,
    CreationMethod CreationMethod,
    string Name,
    string? Uco = null,
    string? Email = null,
    string? Phone = null) : IEntity;

public class AuthorProjection : SingleStreamAggregation<Author>
{
    public AuthorProjection()
    {
    }

    public Author Create(IEvent<AuthorCreated> e)
    {
        return new Author(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name);
    }

    public Author Apply(AuthorInfoChanged e, Author a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            Uco = e.Uco ?? a.Uco,
            Email = e.Email ?? a.Email,
            Phone = e.Phone ?? a.Phone
        };
    }
}
