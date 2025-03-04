using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(KafeDateTimeJsonConverter))]
public record KafeDateTime(
    DateTimeOffset? Value
);

public class KafeDateTimeJsonConverter : JsonConverter<KafeDateTime>
{
    public override KafeDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<DateTimeOffset?>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, KafeDateTime value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
