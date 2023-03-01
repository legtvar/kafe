using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ArtifactInfo(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name) : IEntity;

public class ArtifactInfoProjection : SingleStreamAggregation<ArtifactInfo>
{
    public ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name);
    }

    public ArtifactInfo Apply(ArtifactInfoChanged e, ArtifactInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name
        };
    }
}
