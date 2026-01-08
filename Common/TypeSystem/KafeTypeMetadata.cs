using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Kafe;

public record KafeTypeMetadata(
    KafeType KafeType,
    Type DotnetType,
    KafeTypeAccessibility Accessibility,
    LocalizedString? Title,
    JsonConverter? Converter,
    object? Extension
// TODO: OpenAPI schema override
);
