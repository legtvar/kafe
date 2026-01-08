using System.Collections.Immutable;

namespace Kafe;

public record ScalarTypeMetadata(
    ImmutableArray<IRequirement> DefaultRequirements
);
