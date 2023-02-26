using Kafe.Media;
using System.Collections.Immutable;

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
) : IImageShardEvent;

public record ImageShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
) : IImageShardEvent;
