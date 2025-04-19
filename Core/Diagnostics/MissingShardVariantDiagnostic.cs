
namespace Kafe.Core.Diagnostics;

public record MissingShardVariantDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string Variant
) : IDiagnosticPayload
{
    public static string Name { get; } = "missing-shard-variant";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Shard Variant"),
        (Const.CzechCulture, "Chybí varianta střípku")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard '{ShardName}' is missing the '{Variant}' variant. It may be corrupted."),
        (
            Const.CzechCulture,
            "Střípek '{ShardName}' neobsahuje požadovanou variantu '{Variant}'. "
                + "Toto může znamenat, že střípek je poškozen."
        )
    );
}
