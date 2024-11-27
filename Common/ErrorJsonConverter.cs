using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class ErrorJsonConverter : JsonConverter<Error>
{
    public bool ShouldWriteStackTraces { get; set; } = false;

    public override Error Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Error>(ref reader, options);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Error value,
        JsonSerializerOptions options)
    {
        if (!ShouldWriteStackTraces)
        {
            // NB: We set StackTrace to null! so that the property gets ignored by the default JSON serializer.
            value = value with { StackTrace = null! };
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}
