using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class ReflectionTypeJsonConverter(KafeTypeRegistry typeRegistry) : JsonConverter<Type>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(Type));
    }

    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var kafeType = JsonSerializer.Deserialize<KafeType>(ref reader, options);
        if (!typeRegistry.Types.TryGetValue(kafeType, out var metadata))
        {
            throw new JsonException($"The specified KAFE object type '{kafeType}' is not registered.");
        }

        return metadata.DotnetType;
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        if (!typeRegistry.DotnetTypeMap.TryGetValue(value, out var kafeType))
        {
            throw new JsonException($"The specified System.Type '{value.FullName}' is not registered.");
        }

        writer.WriteStringValue(kafeType.ToString());
    }
}
