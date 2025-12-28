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

    public bool IsValid => Id != KafeType.Invalid;

    public KafeType Id { get; init; } = KafeType.Invalid;

    public string Text { get; init; } = Const.InvalidId;

    public DiagnosticSeverity Severity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ImmutableDictionary<string, object> Arguments { get; init; }
        = ImmutableDictionary<string, object>.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Text);
        sb.Append(" (");
        sb.Append(Id);
        sb.Append(')');
        return sb.ToString();
    }


}
