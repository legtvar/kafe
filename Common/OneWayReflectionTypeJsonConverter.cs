using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// Serialized <see cref="Type"/>s using their <see cref="Type.FullName"/>s. Does NOT deserialize.
/// </summary>
public class OneWayReflectionTypeJsonConverter : JsonConverter<Type>
{
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException(
            "The deserialization of a System.Type is not supported due to security concerns. "
            + "This usage is likely a bug."
        );
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.FullName);
    }
}
