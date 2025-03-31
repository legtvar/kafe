using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class KafeTypeJsonConverter : JsonConverter<KafeType>
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

        return kafeType;
    }

    public override void Write(
        Utf8JsonWriter writer,
        KafeType value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}
