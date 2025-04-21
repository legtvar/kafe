using System;

namespace Kafe.Media.Diagnostics;

public record VideoBitrateTooLowDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    long Bitrate,
    long Min
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "video-bitrate-too-low";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Video Bitrate Too Low"),
        (Const.CzechCulture, "Video má příliš nízký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of video stream #{StreamIndex} of '{ShardName}' is too low. "
                + "It is {Bitrate:bps} but the minimum required video bitrate is {Min:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate video proudu #{StreamIndex} střípku '{ShardName}' je příliš nízký. "
                + "Je roven {Bitrate:bps}, ale minimální požadováný video bitrate je {Min:bps}."
        )
    );
}
