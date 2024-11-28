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
        var value = reader.GetString();
        if (value is null)
        {
            return null;
        }

        if (!Hrib.TryParse(value, out var result, out var error))
        {
            throw new JsonException(error);
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        Hrib value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(throwOnInvalidAndEmpty: false), options);
    }

    public override Hrib ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = Read(ref reader, typeToConvert, options)
            ?? throw new JsonException("Could not parse as Hrib.");
        return value;
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        Hrib value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
