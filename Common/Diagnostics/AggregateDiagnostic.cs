using System.Collections.Immutable;

namespace Kafe;

public record AggregateDiagnostic(
    ImmutableArray<Diagnostic> Inner
) : IDiagnosticPayload
{
    public static string Moniker => "aggregate";

    public static DiagnosticSeverity DefaultSeverity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Aggregate Diagnostic"),
        (Const.CzechCulture, "Hromadné hlášení")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Several ({Count}) diagnostics were reported."),
        (Const.CzechCulture, "Je k dispozici {Count} hlášení.")
    );
}
