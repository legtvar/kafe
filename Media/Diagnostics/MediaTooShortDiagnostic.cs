using System;

namespace Kafe.Media.Diagnostics;

public record MediaTooShortDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string Variant,
    TimeSpan MinDuration
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "media-too-short";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Media Too Short"),
        (Const.CzechCulture, "Příliš krátké médium")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "Shard '{ShardName}' is too short. Minimum required duration is '{MinDuration:c}'."),
        (Const.CzechCulture, "Střípek '{ShardName}' je příliš krátký. Minimální požadovaná délka je '{MinDuration:c}'.")
    );
}
