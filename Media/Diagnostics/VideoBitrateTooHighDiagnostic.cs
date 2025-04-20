using System;

namespace Kafe.Media.Diagnostics;

public record VideoBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    int Max
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "video-bitrate-too-high";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Video Bitrate Too High"),
        (Const.CzechCulture, "Video má příliš vysoký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of video stream #{StreamIndex} of '{ShardName}' is too high. "
                + "Maximum allowed video bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate video proudu #{StreamIndex} střípku '{ShardName}' je příliš vysoký. "
                + "Maximální povolený video bitrate je {Max:bps}."
        )
    );
}
