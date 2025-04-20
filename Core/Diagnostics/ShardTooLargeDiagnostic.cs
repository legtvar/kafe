namespace Kafe.Core.Diagnostics;

public record ShardTooLargeDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    int MaxLength,
    string? Variant = null
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "shard-too-large";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Shard Too Large"),
        (Const.CzechCulture, "Příliš velký střípek")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
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
