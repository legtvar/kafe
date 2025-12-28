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
public record struct Diagnostic : IFormattable, IInvalidable<Diagnostic>
{
    private LocalizedString? message = null;

    public static readonly Diagnostic Invalid = new Diagnostic()
    {
        Descriptor = DiagnosticDescriptor.Invalid,
        Severity = default,
        StackTrace = string.Empty,
    };

    static Diagnostic IInvalidable<Diagnostic>.Invalid => Invalid;

    public Diagnostic(
        IDiagnosticPayload payload,
        DiagnosticDescriptor? descriptorOverride = null,
        DiagnosticSeverity? severityOverride = null,
        string? stackTrace = null,
        int skipFrames = 1
    )
    {
        var descriptor = descriptorOverride;
        descriptor ??= DiagnosticDescriptor.FromPayloadType(payload.GetType());
        Debug.Assert(payload.GetType() == descriptor.Value.PayloadType);

        Descriptor = descriptor.Value;
        Payload = payload;
        Severity = severityOverride ?? descriptor.Value.Severity;
        StackTrace = stackTrace ?? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString();
    }

    public readonly bool IsValid => IInvalidable.GetIsValid(Payload);

    public DiagnosticDescriptor Descriptor { get; init; }

    public IDiagnosticPayload Payload { get; init; }

    public DiagnosticSeverity Severity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StackTrace { get; init; }

    public string ToString(CultureInfo culture)
    {
        if (message is null)
        {
            message = Descriptor.GetMessage(Payload);
        }

        return message[culture];
    }

    public override string ToString()
    {
        return ToString(CultureInfo.InvariantCulture);
    }

    string IFormattable.ToString(string? _, IFormatProvider? formatProvider)
    {
        if (formatProvider is not CultureInfo culture)
        {
            throw new ArgumentException("Format provider must be a CultureInfo.", nameof(formatProvider));
        }

        return ToString(culture);
    }
}
