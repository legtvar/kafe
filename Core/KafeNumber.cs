using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(KafeNumberJsonConverter))]
public record KafeNumber(
    decimal? Value
);

public class KafeNumberJsonConverter : JsonConverter<KafeNumber>
{
    public override KafeNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<decimal?>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, KafeNumber value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
