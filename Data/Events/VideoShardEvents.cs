using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record VideoShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    string ArtifactId
);

public record VideoShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<VideoQualityPreset> Variants
);