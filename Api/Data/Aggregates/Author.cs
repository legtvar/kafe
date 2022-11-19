using Marten.Events.Aggregation;
using Kafe.Data.Events;
using Marten.Events;

namespace Kafe.Data.Aggregates;

public record Author(
    string Id = Hrib.Invalid,
    CreationMethod CreationMethod = CreationMethod.Unknown,
    string? Name = null,
    string? Uco = null,
    string? Email = null,
    string? Phone = null);

public class AuthorProjection : SingleStreamAggregation<Author>
{
    public AuthorProjection()
    {
    }

    public Author Create(IEvent<AuthorCreated> e)
    {
        return new Author(Id: e.StreamKey!, CreationMethod: e.Data.CreationMethod);
    }

    public Author Apply(AuthorInfoChanged e, Author a)
    {
        return a with
        {
            Name = e.Name,
            Uco = e.Uco,
            Email = e.Email,
            Phone = e.Phone
        };
    }
}
