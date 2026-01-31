using System;

namespace Kafe.Core.Diagnostics;

public record ShardAnalysisFailedDiagnostic(
    Type ShardType
) : IDiagnosticPayload
{
    public static string Moniker => "shard-analysis-failed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Analysis Failed"),
        (Const.CzechCulture, "Analýza střípku selhala")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "The provided data cannot be analyzed as {ShardType:H}. "
            + "Is the file correct and uncorrupted?"),
        (Const.CzechCulture, "{ShardType:H} se nepodařilo zanalyzovat. Je soubor správný a nepoškozený?")
    );

    public string? Reason { get; init; }
}
