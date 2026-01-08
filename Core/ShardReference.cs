using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Core;

[JsonConverter(typeof(ShardReferencePropertyJsonConverter))]
public record ShardReference(
    Hrib ShardId
) : IScalar
{
    public static string Moniker { get; } = "shard-ref";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard reference"),
        (Const.CzechCulture, "Odkaz na střípek")
    );
}

public class ShardReferencePropertyJsonConverter : JsonConverter<ShardReference>
{
    public override ShardReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var hrib = JsonSerializer.Deserialize<Hrib?>(ref reader, options)
            ?? throw new JsonException("A shard reference may never be null.");
        if (hrib.IsInvalid || hrib.IsEmpty)
        {
            throw new JsonException("An empty or invalid HRIB is not a valid shard reference.");
        }

        return new(hrib);
    }

    public override void Write(Utf8JsonWriter writer, ShardReference value, JsonSerializerOptions options)
    {
        if (value.ShardId.IsInvalid || value.ShardId.IsEmpty)
        {
            throw new JsonException("An empty or invalid HRIB is not a valid shard reference.");
        }

        JsonSerializer.Serialize(writer, value.ShardId.ToString(throwOnInvalidAndEmpty: false), options);
    }
}
