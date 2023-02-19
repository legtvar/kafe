using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record SubtitlesShard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId,
    ImmutableArray<string> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Subtitles;
}


public class SubtitlesShardProjection : SingleStreamAggregation<SubtitlesShard>
{
    public SubtitlesShardProjection()
    {
    }

    public SubtitlesShard Create(SubtitlesShardCreated e)
    {
        return new(
            Id: e.ShardId,
            CreationMethod: e.CreationMethod,
            ArtifactId: e.ArtifactId,
            Variants: ImmutableArray.Create(CultureInfo.InvariantCulture.TwoLetterISOLanguageName));
    }

    public SubtitlesShard Apply(SubtitlesShardVariantsAdded e, SubtitlesShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }
}