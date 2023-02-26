using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    VideoShardVariant OriginalVariant
) : IShardCreated;

public record VideoShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IShardModified;

public record VideoShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<VideoShardVariant> Variants
) : IShardModified;