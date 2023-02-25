using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId
) : IShardCreated;

public record SubtitlesShardVariant(
    string Name);

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
);

public record SubtitlesShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<SubtitlesShardVariant> Variants
);
