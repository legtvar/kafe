using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record Artifact(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name) : IEntity;

public class ArtifactProjection : SingleStreamAggregation<Artifact>
{
    public Artifact Create(ArtifactCreated e)
    {
        return new Artifact(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact a)
    {
        return a with
        {
            Name = e.Name ?? a.Name
        };
    }
}
