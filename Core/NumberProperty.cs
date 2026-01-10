using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(NumberPropertyJsonConverter))]
public record NumberProperty(
    decimal? Value
) : IScalar
{
    public static string Moniker => "number";
}

public class NumberPropertyJsonConverter : JsonConverter<NumberProperty>
{
    public override NumberProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<decimal?>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, NumberProperty value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
