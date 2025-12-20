using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// A user-facing structure representing an error, a warning, or a less severe note.
/// Compared to exception messages, these must be localized as they are sent and may be displayed to the user through
/// the client.
/// </summary>
public record struct Diagnostic : IFormattable, IInvalidable
{
    private DiagnosticMessage? invariantMessage;

    public static readonly Diagnostic Invalid = new Diagnostic()
    {
        Descriptor = DiagnosticDescriptor.Invalid,
        Payload = KafeObject.Invalid,
        Severity = default,
        StackTrace = string.Empty,
    };

    public Diagnostic(
        DiagnosticDescriptor descriptor,
        KafeObject payload,
        DiagnosticSeverity? severity = null,
        string? stackTrace = null,
        int skipFrames = 1
    )
    {
        Descriptor = descriptor;
        Payload = payload;
        Severity = severity ?? descriptor.DefaultSeverity;
        StackTrace = stackTrace ?? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString();
    }

    public readonly bool IsValid => Descriptor?.IsValid == true && Payload.IsValid;

    public DiagnosticDescriptor Descriptor { get; init; }

    public KafeObject Payload { get; init; }

    public DiagnosticSeverity Severity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StackTrace { get; init; }

    public DiagnosticMessage InvariantMessage
    {
        get
        {
            invariantMessage ??= ToMessage(CultureInfo.InvariantCulture);
            return invariantMessage.Value;
        }
    }

    public DiagnosticMessage ToMessage(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    string IFormattable.ToString(string? _, IFormatProvider? formatProvider)
    {
        if (formatProvider is not CultureInfo culture)
        {
            throw new ArgumentException("Format provider must be a CultureInfo.", nameof(formatProvider));
        }

        return ToMessage(culture).ToString();
    }

    public override string ToString()
    {
        return InvariantMessage.ToString();
    }
}
