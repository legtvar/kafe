using System;

namespace Kafe.Media.Diagnostics;

public record MediaBitrateTooLowDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int Min
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "media-bitrate-too-low";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Media Bitrate Too Low"),
        (Const.CzechCulture, "Médium má příliš nízký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The total bitrate of shard '{ShardName}' is too low. Minimum required bitrate is {Min:bps}."
        ),
        (
            Const.CzechCulture,
            "Celkový bitrate střípku '{ShardName}' je příliš nízký. Minimální požadovaný bitrate je {Min:bps}."
        )
    );
}
