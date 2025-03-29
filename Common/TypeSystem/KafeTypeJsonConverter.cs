using System;
using System.Linq;
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
        var result = KafeType.Parse(value);
        if (result.HasErrors)
        {
            throw new JsonException(result.Errors.First().Message);
        }

        return result.Value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        KafeType value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}
