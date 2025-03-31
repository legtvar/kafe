namespace Kafe.Core.Diagnostics;

public record ShardAnalysisFailureDiagnostic(
    KafeType ShardType
)
{
    public const string Id = "shard-analysis-failure";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Analysis Failure"),
        (Const.CzechCulture, "Selhání analýzy střípku")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "The provided data cannot be analyzed as {ShardType:H}. "
            + "Is the file correct and uncorrupted?"),
        (Const.CzechCulture, "{ShardType:H} se nepodařilo zanalyzovat. Je soubor správný a nepoškozený?")
    );
}
