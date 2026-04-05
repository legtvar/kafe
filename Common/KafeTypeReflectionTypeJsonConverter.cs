using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class KafeTypeReflectionTypeJsonConverter(KafeTypeRegistry typeRegistry) : JsonConverter<Type>
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsAssignableTo(typeof(Type)))
        {
            return false;
        }

        return typeRegistry.DotnetTypeMap.ContainsKey(typeToConvert);
    }

    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        // TODO: Improve performance.
        var type = typeRegistry.DotnetTypeMap.Keys.FirstOrDefault(t => t.FullName == name);
        if (type is null)
        {
            throw new JsonException($"Type '{name}' is not a registered KafeType.");
        }

        return type;
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        var kafeType = typeRegistry.RequireType(value);
        writer.WriteStringValue(kafeType.ToString());
    }
}
