using Kafe.Media;

namespace Kafe.Data.Events;

public interface IVideoShardEvent : IShardEvent
{
}

public record VideoShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    MediaInfo OriginalVariantInfo
) : IVideoShardEvent, IShardCreated;

public record VideoShardVariantsAdded(
    [Hrib] string ShardId,
    string Name,
    MediaInfo Info
) : IVideoShardEvent, IShardVariantAdded;

public record VideoShardVariantsRemoved(
    [Hrib] string ShardId,
    string Name
) : IVideoShardEvent, IShardVariantRemoved;
