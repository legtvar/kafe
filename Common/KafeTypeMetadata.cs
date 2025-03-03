using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Kafe;

public record KafeTypeMetadata
{
    public JsonConverter? Converter { get; init; }

    public KafeTypeUsage Usage { get; init; }

    public ImmutableArray<IRequirement> DefaultRequirements { get; init; }
}
