using System.Collections.Immutable;

namespace Kafe;

public record ModMetadata(
    string Name,
    ImmutableHashSet<KafeType> Types
);
