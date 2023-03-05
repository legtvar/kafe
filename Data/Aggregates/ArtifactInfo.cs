using Kafe.Data.Events;
using Marten.Events.Aggregation;
using System;

namespace Kafe.Data.Aggregates;

public record ArtifactInfo(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    DateTimeOffset AddedOn
) : IEntity;

public class ArtifactInfoProjection : SingleStreamAggregation<ArtifactInfo>
{
    public ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn);
    }

    public ArtifactInfo Apply(ArtifactInfoChanged e, ArtifactInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            AddedOn = e.AddedOn ?? a.AddedOn
        };
    }
}
