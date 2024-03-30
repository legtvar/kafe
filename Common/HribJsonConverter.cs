using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class HribJsonConverter : JsonConverter<Hrib>
{
    public override Hrib? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return (Hrib?)reader.GetString();
    }

    public override void Write(
        Utf8JsonWriter writer,
        Hrib value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }

    public override Hrib ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var result = Read(ref reader, typeToConvert, options);
        if (result is null)
        {
            throw new JsonException("Could not parse as Hrib.");
        }

        return result;
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        Hrib value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value);
    }
}
