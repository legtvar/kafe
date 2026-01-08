using System;
using System.Collections.Immutable;

namespace Kafe;

public record ShardPayloadTypeMetadata(
    ImmutableArray<Type> AnalyzerTypes
);
