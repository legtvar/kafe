using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record VideoShardInfo(
    string Id,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImmutableArray<VideoShardVariant> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Video;
}


public class VideoShardInfoProjection : SingleStreamAggregation<VideoShardInfo>
{
    public VideoShardInfoProjection()
    {
    }

    public VideoShardInfo Create(VideoShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableArray.Create(e.OriginalVariant));
    }

    public VideoShardInfo Apply(VideoShardVariantsAdded e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }

    public VideoShardInfo Apply(VideoShardVariantsRemoved e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Except(s.Variants).ToImmutableArray()
        };
    }
}