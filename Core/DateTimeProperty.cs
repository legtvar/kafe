using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(DateTimePropertyJsonConverter))]
public record DateTimeProperty(
    DateTimeOffset? Value
) : IScalar
{
    public static string Moniker => "date-time";
}

public class DateTimePropertyJsonConverter : JsonConverter<DateTimeProperty>
{
    public override DateTimeProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<DateTimeOffset?>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, DateTimeProperty value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
