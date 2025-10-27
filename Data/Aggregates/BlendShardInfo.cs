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

public record BlendShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    DateTimeOffset CreatedAt,
    [Hrib] string ArtifactId,
    ImmutableDictionary<string, BlendInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Blend;
}


public class BlendShardInfoProjection : SingleStreamProjection<BlendShardInfo, string>
{
    public BlendShardInfoProjection()
    {
    }

    public static BlendShardInfo Create(IEvent<BlendShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, BlendInfo>[]
            {
                new(Const.OriginalShardVariant, e.Data.OriginalVariantInfo)
            })
        );
    }

    public BlendShardInfo Apply(BlendShardVariantAdded e, BlendShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public BlendShardInfo Apply(BlendShardVariantRemoved e, BlendShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}
