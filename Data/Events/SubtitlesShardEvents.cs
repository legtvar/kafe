using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    SubtitlesShardVariant OriginalVariant
) : IShardCreated;

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : IShardModified;

public record SubtitlesShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
) : IShardModified;
