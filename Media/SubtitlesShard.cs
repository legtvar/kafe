using System.Collections.Immutable;

namespace Kafe.Media;

public record SubtitlesShard : IMergeable<SubtitlesShard>
{
    public ImmutableDictionary<string, SubtitlesInfo> Variants { get; init; }
        = ImmutableDictionary<string, SubtitlesInfo>.Empty;

    public object MergeWith(SubtitlesShard other)
    {
        return new SubtitlesShard
        {
            Variants = Variants.SetItems(other.Variants)
        };
    }
}
