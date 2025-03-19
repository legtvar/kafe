using System;
using System.Collections.Immutable;

namespace Kafe;

public record ShardTypeMetadata(
    KafeType KafeType,
    ImmutableArray<Type> AnalyzerTypes
);
