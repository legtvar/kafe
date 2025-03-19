using System.Collections.Immutable;

namespace Kafe;

public record PropertyTypeMetadata(
    KafeType KafeType,
    ImmutableArray<IRequirement> DefaultRequirements
);
