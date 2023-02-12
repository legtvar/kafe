using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record ImageShard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId,
    ImmutableArray<ImageQualityPreset> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Image;
}


public class ImageShardProjection : SingleStreamAggregation<ImageShard>
{
    public ImageShardProjection()
    {
    }

    public ImageShard Create(IEvent<ImageShardCreated> e)
    {
        return new(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            Variants: ImmutableArray.Create(ImageQualityPreset.Original));
    }

    public ImageShard Apply(ImageShardVariantsAdded e, ImageShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }
}
