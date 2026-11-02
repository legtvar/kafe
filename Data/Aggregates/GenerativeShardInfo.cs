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

public record GenerativeShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt,
    ImmutableDictionary<string, GenerativeInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Generative;
}


public class GenerativeShardInfoProjection : SingleStreamProjection<GenerativeShardInfo, string>
{
    public GenerativeShardInfoProjection()
    {
    }

    public static GenerativeShardInfo Create(IEvent<GenerativeShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, GenerativeInfo>[]
            {
                new(Const.OriginalShardVariant, e.Data.OriginalVariantInfo)
            })
        );
    }

    public GenerativeShardInfo Apply(GenerativeShardVariantAdded e, GenerativeShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public GenerativeShardInfo Apply(GenerativeShardVariantRemoved e, GenerativeShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}
