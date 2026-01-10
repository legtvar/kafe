
namespace Kafe.Core.Diagnostics;

public record CorruptedShardDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant = null
) : IDiagnosticPayload
{
    public static string Moniker => "corrupted-shard";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Corrupted Shard"),
        (Const.CzechCulture, "Poškozený střípek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard '{ShardName}' is corrupted."),
        (Const.CzechCulture, "Střípek '{ShardName}' je poškozený.")
    );
}
