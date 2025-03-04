using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Kafe;

public record KafeTypeMetadata(
    KafeType KafeType,
    Type DotnetType,
    KafeTypeUsage Usage,
    KafeTypeAccessibility Accessibility,
    ImmutableArray<IRequirement> DefaultRequirements,
    JsonConverter? Converter
// TODO: OpenAPI schema override
);
