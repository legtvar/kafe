using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public sealed record KafeObject(
    KafeType Type,
    object Value
);

public class KafeObjectJsonConverter : JsonConverter<KafeObject>
{
    private readonly KafeTypeRegistry typeRegistry;

    public KafeObjectJsonConverter(KafeTypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;
    }
    
    public override KafeObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, KafeObject value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
