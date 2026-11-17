using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// Accepts integral JSON floating-point numbers as Int64 values.
/// </summary>
public sealed class FloatToIntJsonConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Expected a JSON number, got {reader.TokenType}.");
        }

        if (reader.TryGetInt64(out var integer))
        {
            return integer;
        }

        var value = reader.GetDecimal();
        if (value != decimal.Truncate(value))
        {
            throw new JsonException($"The JSON number {value} is not an integer.");
        }

        try
        {
            return checked((long)value);
        }
        catch (OverflowException e)
        {
            throw new JsonException($"The JSON number {value} is outside the Int64 range.", e);
        }
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
