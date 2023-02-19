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
    LocalizedString Name,
    ImmutableArray<string> ShardIds) : IEntity;

public class ArtifactProjection : MultiStreamAggregation<Artifact, string>
{
    public ArtifactProjection()
    {
        Identity<VideoShardCreated>(e => e.ArtifactId);
        Identity<ImageShardCreated>(e => e.ArtifactId);
        Identity<SubtitlesShardCreated>(e => e.ArtifactId);
    }

    public Artifact Create(IEvent<ArtifactCreated> e)
    {
        return new Artifact(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            ShardIds: ImmutableArray<string>.Empty);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact a)
    {
        return a with
        {
            Name = e.Name ?? a.Name
        };
    }

    public Artifact Apply(IEvent<VideoShardCreated> e, Artifact a)
    {
        if (a.ShardIds.Contains(e.StreamKey!))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.StreamKey!)
        };
    }

    public Artifact Apply(IEvent<ImageShardCreated> e, Artifact a)
    {
        if (a.ShardIds.Contains(e.StreamKey!))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.StreamKey!)
        };
    }

    public Artifact Apply(IEvent<SubtitlesShardCreated> e, Artifact a)
    {
        if (a.ShardIds.Contains(e.StreamKey!))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.StreamKey!)
        };
    }
}
