using System.Collections.Immutable;

namespace Kafe.Core.Diagnostics;

public record AggregateDiagnostic(
    ImmutableArray<Diagnostic> Inner
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "aggregate";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Aggregate Diagnostic"),
        (Const.CzechCulture, "Hromadné hlášení")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Several ({Count}) diagnostics were reported."),
        (Const.CzechCulture, "Je k dispozici {Count} hlášení.")
    );

    public int Count => Inner.Length;
}
