using System.Collections.Immutable;

namespace Kafe.Core.Diagnostics;

public record AggregateDiagnostic(
    ImmutableArray<Diagnostic> Inner,
    long Count
)
{
    public const string Id = "aggregate";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Aggregate Diagnostic"),
        (Const.CzechCulture, "Několik hlášení")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "Several ({Count}) diagnostics were reported."),
        (Const.CzechCulture, "Je k dispozici {Count} hlášení.")
    );
}
