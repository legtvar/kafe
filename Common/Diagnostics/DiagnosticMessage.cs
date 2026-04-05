using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// A culture-specific <see cref="Diagnostic"/> structure meant to be serialized and sent.
/// </summary>
public readonly record struct DiagnosticMessage : IInvalidable<DiagnosticMessage>
{
    public static DiagnosticMessage Invalid => new DiagnosticMessage();

    public DiagnosticMessage()
    {
    }

    [JsonIgnore]
    public bool IsValid => Type.IsValid;

    public KafeType Type { get; init; } = KafeType.Invalid;

    public string Title { get; init; } = Const.InvalidId;

    public string Message { get; init; } = Const.InvalidId;

    public DiagnosticSeverity Severity { get; init; }

    public string Language { get; init; } = Const.InvariantCultureCode;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ImmutableDictionary<string, object?> Arguments { get; init; }
        = ImmutableDictionary<string, object?>.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Severity);
        sb.Append(" [");
        sb.Append(Type);
        sb.Append(']');
        sb.Append(": ");
        sb.Append(Title);
        sb.Append(": ");
        sb.Append(Message);
        return sb.ToString();
    }


}
