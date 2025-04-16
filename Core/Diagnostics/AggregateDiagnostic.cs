using System.Collections.Immutable;
using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "aggregate")]
public record AggregateDiagnostic(
    ImmutableArray<Diagnostic> Inner,
    long Count
) : IDiagnosticPayload
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Aggregate Diagnostic"),
        (Const.CzechCulture, "Několik hlášení")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Several ({Count}) diagnostics were reported."),
        (Const.CzechCulture, "Je k dispozici {Count} hlášení.")
    );
}
