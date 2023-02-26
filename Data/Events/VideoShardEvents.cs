using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public interface IVideoShardEvent : IShardEvent
{
}

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    VideoShardVariant OriginalVariant
) : IVideoShardEvent, IShardCreated;

public record VideoShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IVideoShardEvent;

public record VideoShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IVideoShardEvent;
