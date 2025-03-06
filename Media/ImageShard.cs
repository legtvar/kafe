using System.Collections.Immutable;

namespace Kafe.Media;

public record ImageShard : IMergeable<ImageShard>
{
    public ImmutableDictionary<string, ImageInfo> Variants { get; init; }
        = ImmutableDictionary<string, ImageInfo>.Empty;

    public object MergeWith(ImageShard other)
    {
        return new ImageShard
        {
            Variants = Variants.SetItems(other.Variants)
        };
    }
}
