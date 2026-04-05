using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe.Diagnostics;

/// <summary>
/// Converts <see cref="Diagnostic"/>s to <see cref="DiagnosticMessage"/>s and back.
/// </summary>
public class DiagnosticMessageJsonConverter(KafeTypeRegistry typeRegistry) : JsonConverter<Diagnostic>
{
    public override Diagnostic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var message = JsonSerializer.Deserialize<DiagnosticMessage>(ref reader, options);
        return Diagnostic.FromDiagnosticMessage(typeRegistry, message);
    }

    public override void Write(Utf8JsonWriter writer, Diagnostic value, JsonSerializerOptions options)
    {
        var message = value.ToDiagnosticMessage(typeRegistry, CultureInfo.CurrentCulture);
        JsonSerializer.Serialize(writer, message, options);
    }
}
