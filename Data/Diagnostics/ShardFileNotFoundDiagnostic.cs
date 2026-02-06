namespace Kafe.Data.Diagnostics;

public record ShardFileNotFoundDiagnostic(
    Hrib ShardId
) : IDiagnosticPayload
{
    public static string Moniker => "shard-file-not-found";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard File Not Found"),
        (Const.CzechCulture, "Soubor střípku nenalezen")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Could not find the file for shard '{ShardId}'."),
        (Const.CzechCulture, "Soubor střípku '{ShardId}' nebylo možné nalézt.")
    );
}
