using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class KafeTypeJsonConverter(KafeTypeRegistry typeRegistry) : JsonConverter<KafeType>
{
    public override KafeType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new JsonException("A KAFE type may never be null.");
        if (!KafeType.TryParse(value, out var kafeType))
        {
            throw new JsonException("Could not parse into a KafeType.");
        }

        // NB: in case it is an array
        var itemType = kafeType with { IsArray = false };

        return !typeRegistry.Types.TryGetValue(itemType, out _)
            ? throw new JsonException("This type is not registered.")
            : kafeType;
    }

    public override void Write(
        Utf8JsonWriter writer,
        KafeType value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}
