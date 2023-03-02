using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record VideoShardInfo(
    string Id,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImmutableDictionary<string, MediaInfo> Variants
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
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, MediaInfo>[]
            {
                new(Const.OriginalShardVariant, e.OriginalVariantInfo)
            })
        );
    }

    public VideoShardInfo Apply(VideoShardVariantsAdded e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public VideoShardInfo Apply(VideoShardVariantsRemoved e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}