using System.Collections.Immutable;

namespace Kafe.Media;

public record ImageShard
{
    public ImmutableDictionary<string, ImageInfo> Variants { get; init; }
        = ImmutableDictionary<string, ImageInfo>.Empty;
}
