using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

public class KafeObjectJsonConverterFactory(KafeTypeRegistry typeRegistry, JsonConverter<KafeObject> inner)
    : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(KafeObject) || typeToConvert == typeof(KafeObject?);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = new KafeObjectJsonConverter(typeRegistry, inner);

        if (typeToConvert == typeof(KafeObject))
        {
            return converter;
        }

        if (typeToConvert == typeof(KafeObject?))
        {
            return new NullableKafeObjectJsonConverter(converter);
        }

        return null;
    }
}

public class KafeObjectJsonConverter(KafeTypeRegistry typeRegistry, JsonConverter<KafeObject> inner)
    : JsonConverter<KafeObject>
{
    public override KafeObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected the start of an object.");
        }

        var typeReader = reader;
        var type = KafeType.Invalid;
        while (!type.IsValid && typeReader.Read())
        {
            if (typeReader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = typeReader.GetString();
            if (propertyName?.ToLowerInvariant() != nameof(KafeObject.Type).ToLowerInvariant())
            {
                typeReader.Skip();
                continue;
            }

            typeReader.Read();
            type = JsonSerializer.Deserialize<KafeType>(ref typeReader, options);
            break;
        }

        if (!type.IsValid)
        {
            throw new JsonException("The type of the object must be specified.");
        }

        if (!typeRegistry.TryGetDotnetType(type, out var dotnetType))
        {
            throw new JsonException("The specified type is not registered.");
        }

        object? value = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            if (propertyName?.ToLowerInvariant() != nameof(KafeObject.Value).ToLowerInvariant())
            {
                reader.Skip();
                continue;
            }

            reader.Read();
            value = JsonSerializer.Deserialize(ref reader, dotnetType, options);
            break;
        }

        if (value is null)
        {
            throw new JsonException("The value of a KAFE object cannot be null.");
        }

        // TODO: Read DotnetType and perform upcasting.

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            reader.Skip();
        }

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Expected the end of an object.");
        }

        return new KafeObject(type, value);
    }

    public override void Write(Utf8JsonWriter writer, KafeObject value, JsonSerializerOptions options)
    {
        inner.Write(writer, value, options);
    }
}

public class NullableKafeObjectJsonConverter(KafeObjectJsonConverter inner)
    : JsonConverter<KafeObject?>
{
    public override bool HandleNull => true;

    public override KafeObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            reader.Read();
            return null;
        }

        return inner.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, KafeObject? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        inner.Write(writer, value.Value, options);
    }
}
