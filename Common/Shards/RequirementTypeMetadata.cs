using System;
using System.Collections.Immutable;

namespace Kafe;

public record RequirementTypeMetadata(
    ImmutableArray<Type> HandlerTypes
);
