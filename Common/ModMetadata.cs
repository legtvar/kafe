using System.Collections.Immutable;

namespace Kafe;

public record ModMetadata
{
    public required string Slug { get; init; }

    public required ImmutableHashSet<KafeType> Types { get; init; }
}
