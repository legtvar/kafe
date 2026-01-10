namespace Kafe.Core.Diagnostics;

public record ShardTooLargeDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    int MaxLength,
    string? Variant = null
) : IDiagnosticPayload
{
    public static string Moniker => "shard-too-large";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Too Large"),
        (Const.CzechCulture, "Příliš velký střípek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Shard '{ShardName}' is too large. Its maximum allowed file length is {MaxLength:FS F2}."
        ),
        (
            Const.CzechCulture,
            "Střípek '{ShardName}' je příliš velký. Jeho maximální povolená velikost na disku je {MaxLength:FS F2}."
        )
    );
}
