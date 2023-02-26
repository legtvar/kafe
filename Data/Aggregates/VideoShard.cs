using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record VideoShard(
    string Id,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImmutableArray<VideoShardVariant> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Video;
}


public class VideoShardProjection : SingleStreamAggregation<VideoShard>
{
    public VideoShardProjection()
    {
    }

    public VideoShard Create(VideoShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableArray.Create(e.OriginalVariant));
    }

    public VideoShard Apply(VideoShardVariantsAdded e, VideoShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }

    public VideoShard Apply(VideoShardVariantsRemoved e, VideoShard s)
    {
        return s with
        {
            Variants = s.Variants.Except(s.Variants).ToImmutableArray()
        };
    }
}