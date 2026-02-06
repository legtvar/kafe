namespace Kafe.Data.Diagnostics;

public class TemporaryShardNotFoundDiagnostic(
    Hrib ShardId
) : IDiagnosticPayload
{
    public static string Moniker => "temporary-shard-not-found";

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Temporary Shard Not Found"),
        (Const.CzechCulture, "Dočasný střípek nenalezen")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Could not find the file for temporary shard '{ShardId}'."),
        (Const.CzechCulture, "Soubor dočasného střípku '{ShardId}' nebylo možné nalézt.")
    );
}
