using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ImageShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId
) : IShardCreated;

public record ImageShardVariant(
    string Name,
    ImageInfo Info
);

public record ImageShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
);

public record ImageShardVariantsRemoved(
    Hrib ShardId,
    ImmutableArray<ImageShardVariant> Variants
);
