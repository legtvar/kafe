using System;

namespace Kafe.Media.Diagnostics;

public record AudioBitrateTooLowDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    int Min
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "audio-bitrate-too-low";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Audio Bitrate Too Low"),
        (Const.CzechCulture, "Audio má příliš nízký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of audio stream #{StreamIndex} of '{ShardName}' is too low. "
                + "Minimum required audio bitrate is {Min:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate audio proudu #{StreamIndex} střípku '{ShardName}' je příliš nízký. "
                + "Minimální požadovaný bitrate audia je {Min:bps}."
        )
    );
}
