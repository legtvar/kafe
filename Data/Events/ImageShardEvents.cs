using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ImageShardCreated(
    CreationMethod CreationMethod,
    string ArtifactId);

public record ImageShardVariantsAdded(
    ImmutableArray<ImageQualityPreset> Variants);
