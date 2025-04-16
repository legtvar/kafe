using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;


[JsonConverter(typeof(ShardReferencePropertyJsonConverter))]
public record ShardReferenceProperty(
    Hrib ShardId
);

public class ShardReferencePropertyJsonConverter : JsonConverter<ShardReferenceProperty>
{
    public override ShardReferenceProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var hrib = JsonSerializer.Deserialize<Hrib?>(ref reader, options)
            ?? throw new JsonException("A shard reference may never be null.");
        if (hrib.IsInvalid || hrib.IsEmpty)
        {
            throw new JsonException("An empty or invalid HRIB is not a valid shard reference.");
        }

        return new(hrib);
    }

    public override void Write(Utf8JsonWriter writer, ShardReferenceProperty value, JsonSerializerOptions options)
    {
        if (value.ShardId.IsInvalid || value.ShardId.IsEmpty)
        {
            throw new JsonException("An empty or invalid HRIB is not a valid shard reference.");
        }

        JsonSerializer.Serialize(writer, value.ShardId.ToString(throwOnInvalidAndEmpty: false), options);
    }
}
