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
    ImmutableArray<string> ShardIds) : IEntity
{
    public Artifact() : this("", default, (LocalizedString)"", ImmutableArray<string>.Empty)
    {
    }
}

public class ArtifactProjection : MultiStreamAggregation<Artifact, Hrib>
{
    public ArtifactProjection()
    {
        Identity<ArtifactCreated>(e => e.ArtifactId);
        Identity<ArtifactInfoChanged>(e => e.ArtifactId);
        Identity<VideoShardCreated>(e => e.ArtifactId);
        Identity<ImageShardCreated>(e => e.ArtifactId);
        Identity<SubtitlesShardCreated>(e => e.ArtifactId);
    }

    public Artifact Create(ArtifactCreated e)
    {
        return new Artifact(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            ShardIds: ImmutableArray<string>.Empty);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact a)
    {
        return a with
        {
            Name = e.Name ?? a.Name
        };
    }

    public Artifact Apply(VideoShardCreated e, Artifact a)
    {
        if (a.ShardIds.Contains(e.ShardId))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.ShardId)
        };
    }

    public Artifact Apply(ImageShardCreated e, Artifact a)
    {
        if (a.ShardIds.Contains(e.ShardId))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.ShardId)
        };
    }

    public Artifact Apply(SubtitlesShardCreated e, Artifact a)
    {
        if (a.ShardIds.Contains(e.ShardId))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.ShardId)
        };
    }
}
