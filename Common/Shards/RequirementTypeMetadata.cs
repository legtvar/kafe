using System;
using System.Collections.Immutable;

namespace Kafe;

public record RequirementTypeMetadata(
    KafeType KafeType,
    ImmutableArray<Type> HandlerTypes
) : ISubtypeMetadata;
