
namespace Kafe.Core.Diagnostics;

public record CorruptedShardVariantDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string Variant
) : IDiagnosticPayload
{
    public static string Name { get; } = "corrupted-shard-variant";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Corrupted Shard Variant"),
        (Const.CzechCulture, "Poškozená varianta střípku")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard '{ShardName}' is corrupted."),
        (Const.CzechCulture, "Střípek '{ShardName}' je poškozený.")
    );
}
