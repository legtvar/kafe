using System.Text.Json.Serialization;

namespace Kafe;

public record KafeTypeMetadata
{
    JsonConverter? Converter { get; init; }
}
