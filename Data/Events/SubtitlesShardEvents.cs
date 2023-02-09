using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record SubtitlesShardCreated(
    CreationMethod CreationMethod,
    string ArtifactId);

public record SubtitlesShardVariantsAdded(
    ImmutableArray<string> Variants);
