using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Kafe;


/// <summary>
/// A user-facing structure representing an error, a warning, or a less severe note.
/// Compared to exception messages, these must be localized as they are sent and may be displayed to the user through
/// the client.
/// </summary>
public record struct Diagnostic : IFormattable
{
    private DiagnosticMessage? invariantMessage;

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

    public DiagnosticDescriptor Descriptor { get; init; }

    public KafeObject Payload { get; init; }

    public DiagnosticSeverity Severity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ImmutableDictionary<string, object> Arguments { get; init; }
        = ImmutableDictionary<string, object>.Empty;

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

    // NB: This operator is "explicit" so that it is not so easy to create multiply nested error-exception-errors
    //     by accident.
    public static explicit operator Exception(Diagnostic error)
    {
        return new KafeErrorException(error);
    }

    public static implicit operator Diagnostic(Exception exception)
    {
        return new Diagnostic(exception);
    }

    public Diagnostic WithArgument(string key, object value)
    {
        return this with
        {
            Arguments = Arguments.Add(key, value)
        };
    }

    public DiagnosticMessage ToMessage(CultureInfo culture)
    {

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
