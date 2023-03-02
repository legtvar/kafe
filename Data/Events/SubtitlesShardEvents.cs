using Kafe.Media;

namespace Kafe.Data.Events;

public interface ISubtitlesShardEvent : IShardEvent
{
}

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    SubtitlesInfo OriginalVariantInfo
) : ISubtitlesShardEvent, IShardCreated;

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    string Name,
    SubtitlesInfo Info
) : ISubtitlesShardEvent, IShardVariantAdded;

public record SubtitlesShardVariantsRemoved(
    Hrib ShardId,
    string Name
) : ISubtitlesShardEvent, IShardVariantRemoved;
