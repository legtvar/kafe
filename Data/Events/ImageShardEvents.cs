using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ImageShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId,
    ImageShardVariant OriginalVariant
) : IShardCreated;

public record ImageShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
) : IShardModified;

public record ImageShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
) : IShardModified;
