namespace Kafe.Data.Diagnostics;

public record ShardTooSmallDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    int MinLength,
    string? Variant = null
) : IDiagnosticPayload
{
    public static string Name { get; } = "shard-too-small";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Too Small"),
        (Const.CzechCulture, "Příliš malý střípek")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
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
