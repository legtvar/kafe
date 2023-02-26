using Kafe.Media;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe.Data.Events;

public interface IImageShardEvent : IShardEvent
{
}

public record ImageShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImageShardVariant OriginalVariant
) : IImageShardEvent, IShardCreated;

public record ImageShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
) : IImageShardEvent, IShardVariantsAdded
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}

public record ImageShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
) : IImageShardEvent, IShardVariantsRemoved
{
    public IEnumerable<string> GetVariantNames()
    {
        return Variants.Select(v => v.Name);
    }
}
