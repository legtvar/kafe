using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record ImageShardInfo(
    string Id,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImmutableDictionary<string, ImageInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Image;
}


public class ImageShardInfoProjection : SingleStreamAggregation<ImageShardInfo>
{
    public ImageShardInfoProjection()
    {
    }

    public ImageShardInfo Create(ImageShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, ImageInfo>[]
            {
                new(Const.OriginalShardVariant, e.OriginalVariantInfo)
            })
        );
    }

    public ImageShardInfo Apply(ImageShardVariantsAdded e, ImageShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public ImageShardInfo Apply(ImageShardVariantsRemoved e, ImageShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}
