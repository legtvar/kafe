using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Artifact(
    string Id,
    ArtifactKind Kind,
    CreationMethod CreationMethod,
    LocalizedString Name,
    string? FileName,
    MediaInfo Metadata) : IEntity;

public class ArtifactProjections : SingleStreamAggregation<Artifact>
{
    public ArtifactProjections()
    {
    }

    public Artifact Create(IEvent<ArtifactCreated> e)
    {
        return new Artifact(
            Id: e.StreamKey!,
            Kind: e.Data.Kind,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            FileName: e.Data.FileName,
            Metadata: e.Data.Metadata);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact v)
    {
        return v with
        {
            Name = e.Name ?? v.Name,
            FileName = e.FileName ?? v.FileName,
            Metadata = e.Metadata ?? v.Metadata
        };
    }
}
