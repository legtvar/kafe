using System.Collections.Immutable;

namespace Kafe;

public record ModMetadata(
    IMod Instance,
    string Name,
    ImmutableHashSet<KafeType> Types
);
