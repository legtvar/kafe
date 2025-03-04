using System.Collections.Immutable;

namespace Kafe.Media;

public record VideoShard
{
    public ImmutableDictionary<string, MediaInfo> Variants { get; init; }
        = ImmutableDictionary<string, MediaInfo>.Empty;
}
