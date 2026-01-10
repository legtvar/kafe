using System;

namespace Kafe.Media.Diagnostics;

public record AudioBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    long Bitrate,
    long Max
) : IDiagnosticPayload
{
    public static string Moniker => "audio-bitrate-too-high";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Audio Bitrate Too High"),
        (Const.CzechCulture, "Audio má příliš vysoký bitrate")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of audio stream #{StreamIndex} of '{ShardName}' is too high. "
                + "It is {Bitrate:bps} but the maximum allowed audio bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate Audio proudu #{StreamIndex} střípku '{ShardName}' je příliš vysoký. "
                + "Je roven {Bitrate:bps}, ale maximální povolený bitrate audia je {Max:bps}."
        )
    );
}
