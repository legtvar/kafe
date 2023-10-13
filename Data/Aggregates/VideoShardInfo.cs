using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record VideoShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    DateTimeOffset CreatedAt,
    [Hrib] string ArtifactId,
    ImmutableDictionary<string, MediaInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Video;
}


public class VideoShardInfoProjection : SingleStreamProjection<VideoShardInfo>
{
    public VideoShardInfoProjection()
    {
    }

    public VideoShardInfo Create(IEvent<VideoShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, MediaInfo>[]
            {
                new(Const.OriginalShardVariant, e.Data.OriginalVariantInfo)
            })
        );
    }

    public VideoShardInfo Apply(VideoShardVariantAdded e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public VideoShardInfo Apply(VideoShardVariantRemoved e, VideoShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}
