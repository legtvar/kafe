using System;
using System.Collections.Immutable;

namespace Kafe;

public record RequirementMetadata(
    KafeType KafeType,
    ImmutableArray<Type> HandlerTypes
);
