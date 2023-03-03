using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Aggregates;

public record TemporaryAccountInfo(
    string Id,
    CreationMethod CreationMethod,

    [property: UniqueIndex(IndexType = UniqueIndexType.Computed)]
    string EmailAddress,

    string PreferredCulture,
    string? SecurityStamp,
    DateTimeOffset RefreshedOn,
    ImmutableHashSet<Hrib> Projects
);

public class TemporaryAccountInfoProjection : SingleStreamAggregation<TemporaryAccountInfo>
{
    public TemporaryAccountInfoProjection()
    {
    }

    public TemporaryAccountInfo Create(TemporaryAccountCreated e)
    {
        return new(
            Id: e.AccountId,
            CreationMethod: e.CreationMethod,
            EmailAddress: e.EmailAddress,
            PreferredCulture: e.PreferredCulture,
            SecurityStamp: null,
            RefreshedOn: default,
            Projects: ImmutableHashSet<Hrib>.Empty
        );
    }

    public TemporaryAccountInfo Apply(IEvent<TemporaryAccountRefreshed> e, TemporaryAccountInfo a)
    {
        return a with
        {
            SecurityStamp = e.Data.SecurityStamp,
            RefreshedOn = e.Timestamp
        };
    }

    public TemporaryAccountInfo Apply(TemporaryAccountClosed e, TemporaryAccountInfo a)
    {
        return a with
        {
            SecurityStamp = null
        };
    }

    public TemporaryAccountInfo Apply(TemporaryAccountInfoChanged e, TemporaryAccountInfo a)
    {
        return a with
        {
            PreferredCulture = e.PreferredCulture ?? a.PreferredCulture
        };
    }

    public TemporaryAccountInfo Apply(AccountProjectAdded e, TemporaryAccountInfo a)
    {
        return a with
        {
            Projects = a.Projects.Add(e.ProjectId)
        };
    }

    public TemporaryAccountInfo Apply(AccountProjectRemoved e, TemporaryAccountInfo a)
    {
        return a with
        {
            Projects = a.Projects.Remove(e.ProjectId)
        };
    }
}
