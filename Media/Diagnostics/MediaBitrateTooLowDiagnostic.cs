using System;

namespace Kafe.Media.Diagnostics;

public record MediaBitrateTooLowDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int Min
) : IDiagnosticPayload
{
    public static string Moniker => "media-bitrate-too-low";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Bitrate Too Low"),
        (Const.CzechCulture, "Médium má příliš nízký bitrate")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
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
