using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId
) : IShardCreated;

public record VideoShardVariant(
    string Name,
    MediaInfo Info
);

public record VideoShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
);

public record VideoShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
);