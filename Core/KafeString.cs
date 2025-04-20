using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(KafeStringJsonConverter))]
public record KafeString(
    string? Value
) : IPropertyType
{
    public static string Moniker { get; } = "string";
}

public class KafeStringJsonConverter : JsonConverter<KafeString>
{
    public override KafeString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<string?>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, KafeString value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
