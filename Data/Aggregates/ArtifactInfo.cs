using Kafe.Data.Events;
using Marten.Events.Aggregation;
using System;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ArtifactInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    DateTimeOffset AddedOn,
    Visibility Visibility
) : IVisibleEntity;

public class ArtifactInfoProjection : SingleStreamProjection<ArtifactInfo>
{
    public ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn,
            Visibility: e.Visibility);
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
