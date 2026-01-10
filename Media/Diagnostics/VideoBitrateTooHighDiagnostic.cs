using System;

namespace Kafe.Media.Diagnostics;

public record VideoBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    long Bitrate,
    long Max
) : IDiagnosticPayload
{
    public static string Moniker => "video-bitrate-too-high";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Video Bitrate Too High"),
        (Const.CzechCulture, "Video má příliš vysoký bitrate")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of video stream #{StreamIndex} of '{ShardName}' is too high. "
                + "It is {Bitrate:bps} but the maximum allowed video bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate video proudu #{StreamIndex} střípku '{ShardName}' je příliš vysoký. "
                + "Je roven {Bitrate:bps}, ale maximální povolený video bitrate je {Max:bps}."
        )
    );
}
