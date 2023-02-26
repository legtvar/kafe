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
    Hrib ArtifactId,
    ImmutableArray<ImageShardVariant> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Image;
}


public class ImageShardProjection : SingleStreamAggregation<ImageShard>
{
    public ImageShardProjection()
    {
    }

    public ImageShard Create(ImageShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableArray.Create(e.OriginalVariant));
    }

    public ImageShard Apply(ImageShardVariantsAdded e, ImageShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }

    public ImageShard Apply(ImageShardVariantsRemoved e, ImageShard s)
    {
        return s with
        {
            Variants = s.Variants.Except(e.Variants).ToImmutableArray()
        };
    }
}
