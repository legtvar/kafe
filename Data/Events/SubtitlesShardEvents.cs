using Kafe.Media;

namespace Kafe.Data.Events;

public interface ISubtitlesShardEvent : IShardEvent
{
}

public record SubtitlesShardCreated(
    [Hrib] string ShardId,
    CreationMethod CreationMethod,
    [Hrib] string ArtifactId,
    SubtitlesInfo OriginalVariantInfo
) : ISubtitlesShardEvent, IShardCreated;

public record SubtitlesShardVariantsAdded(
    [Hrib] string ShardId,
    string Name,
    SubtitlesInfo Info
) : ISubtitlesShardEvent, IShardVariantAdded;

public record SubtitlesShardVariantsRemoved(
    [Hrib] string ShardId,
    string Name
) : ISubtitlesShardEvent, IShardVariantRemoved;
