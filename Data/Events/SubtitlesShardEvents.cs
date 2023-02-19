using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record SubtitlesShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    string ArtifactId
);

public record SubtitlesShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<string> Variants
);
