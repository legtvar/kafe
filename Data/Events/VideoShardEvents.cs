using Kafe.Media;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record VideoShardCreated(
    CreationMethod CreationMethod,
    string ArtifactId);

public record VideoShardVariantsAdded(
    ImmutableArray<VideoQualityPreset> Variants);