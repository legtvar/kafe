namespace Kafe.Core.Diagnostics;

public record ShardAnalysisFailureDiagnostic(
    KafeType ShardType
) : IDiagnosticPayload
{
    public static string Moniker => "shard-analysis-failure";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Analysis Failure"),
        (Const.CzechCulture, "Selhání analýzy střípku")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "The provided data cannot be analyzed as {ShardType:H}. "
            + "Is the file correct and uncorrupted?"),
        (Const.CzechCulture, "{ShardType:H} se nepodařilo zanalyzovat. Je soubor správný a nepoškozený?")
    );
}
