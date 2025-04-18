using System.Collections.Immutable;

namespace Kafe.Core.Diagnostics;

public record AggregateDiagnostic(
    ImmutableArray<Diagnostic> Inner,
    long Count
) : IDiagnosticPayload
{
    public static string Name { get; } = "aggregate";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Aggregate Diagnostic"),
        (Const.CzechCulture, "Několik hlášení")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Several ({Count}) diagnostics were reported."),
        (Const.CzechCulture, "Je k dispozici {Count} hlášení.")
    );
}
