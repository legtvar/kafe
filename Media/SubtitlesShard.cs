using System.Collections.Immutable;

namespace Kafe.Media;

public record SubtitlesShard
{
    public ImmutableDictionary<string, SubtitlesInfo> Variants { get; init; }
        = ImmutableDictionary<string, SubtitlesInfo>.Empty;
}
