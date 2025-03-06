using System.Collections.Immutable;

namespace Kafe.Media;

public record VideoShard : IMergeable<VideoShard>
{
    public ImmutableDictionary<string, MediaInfo> Variants { get; init; }
        = ImmutableDictionary<string, MediaInfo>.Empty;

    public object MergeWith(VideoShard other)
    {
        return new VideoShard
        {
            Variants = Variants.SetItems(other.Variants)
        };
    }
}
