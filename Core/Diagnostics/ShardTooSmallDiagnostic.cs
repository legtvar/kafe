namespace Kafe.Core.Diagnostics;

public record ShardTooSmallDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    int MinLength,
    string? Variant = null
) : IDiagnosticPayload
{
    public static string Moniker => "shard-too-small";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Too Small"),
        (Const.CzechCulture, "Příliš malý střípek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Shard '{ShardName}' is too small. Its minimum required file length is {MinLength:FS F2}."
        ),
        (
            Const.CzechCulture,
            "Střípek '{ShardName}' je příliš malý. Jeho minimalní požadovaná velikost na disku je {MinLength:FS F2}."
        )
    );
}
