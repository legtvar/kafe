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

public record SubtitlesShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    DateTimeOffset CreatedAt,
    ImmutableDictionary<string, SubtitlesInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
{
    public override ShardKind Kind => ShardKind.Subtitles;
}


public class SubtitlesShardInfoProjection : SingleStreamProjection<SubtitlesShardInfo>
{
    public SubtitlesShardInfoProjection()
    {
    }

    public SubtitlesShardInfo Create(IEvent<SubtitlesShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            CreatedAt: e.Timestamp,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, SubtitlesInfo>[]
            {
                new(Const.OriginalShardVariant, e.Data.OriginalVariantInfo)
            })
        );
    }

    public SubtitlesShardInfo Apply(SubtitlesShardVariantsAdded e, SubtitlesShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name).Add(e.Name, e.Info)
        };
    }

    public SubtitlesShardInfo Apply(SubtitlesShardVariantsRemoved e, SubtitlesShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }
}
