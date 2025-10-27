using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using JasperFx.Events;

namespace Kafe.Data.Aggregates;

public record ImageShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt,
    ImmutableDictionary<string, ImageInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Image;
}


public class ImageShardInfoProjection : SingleStreamProjection<ImageShardInfo, string>
{
    public ImageShardInfoProjection()
    {
    }

    public static ImageShardInfo Create(IEvent<ImageShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, ImageInfo>[]
            {
                new(Const.OriginalShardVariant, e.Data.OriginalVariantInfo)
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
