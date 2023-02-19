using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ImageShardCreated(
    Hrib ShardId,
    CreationMethod CreationMethod,
    Hrib ArtifactId);

public record ImageShardVariantsAdded(
    Hrib ShardId,
    ImmutableArray<ImageQualityPreset> Variants);
