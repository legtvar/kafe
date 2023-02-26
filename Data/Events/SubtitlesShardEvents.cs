using System.Collections.Immutable;

namespace Kafe.Data.Events;

public interface ISubtitlesShardEvent : IShardEvent
{
}

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    SubtitlesShardVariant OriginalVariant
) : ISubtitlesShardEvent, IShardCreated;

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : ISubtitlesShardEvent;

public record SubtitlesShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : ISubtitlesShardEvent;
