using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record SubtitlesShardInfo(
    string Id,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImmutableDictionary<string, SubtitlesInfo> Variants
) : ShardInfoBase(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Subtitles;
}


public class SubtitlesShardInfoProjection : SingleStreamAggregation<SubtitlesShardInfo>
{
    public SubtitlesShardInfoProjection()
    {
    }

    public SubtitlesShardInfo Create(SubtitlesShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableDictionary.CreateRange(new KeyValuePair<string, SubtitlesInfo>[]
            {
                new(Const.OriginalShardVariant, e.OriginalVariantInfo)
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