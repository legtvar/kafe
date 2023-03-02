using Kafe.Media;

namespace Kafe.Data.Events;

public interface IVideoShardEvent : IShardEvent
{
}

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    MediaInfo OriginalVariantInfo
) : IVideoShardEvent, IShardCreated;

public record VideoShardVariantsAdded(
    Hrib ShardId,
    string Name,
    MediaInfo Info
) : IVideoShardEvent, IShardVariantAdded;

public record VideoShardVariantsRemoved(
    Hrib ShardId,
    string Name
) : IVideoShardEvent, IShardVariantRemoved;
