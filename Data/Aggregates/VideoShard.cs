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
    string ArtifactId,
    ImmutableArray<VideoQualityPreset> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Video;
}


public class VideoShardProjection : SingleStreamAggregation<VideoShard>
{
    public VideoShardProjection()
    {
    }

    public VideoShard Create(IEvent<VideoShardCreated> e)
    {
        return new(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            Variants: ImmutableArray.Create(VideoQualityPreset.Original));
    }

    public VideoShard Apply(VideoShardVariantsAdded e, VideoShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }
}