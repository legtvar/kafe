using System;

namespace Kafe.Media.Diagnostics;

public record SubtitleBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    long Bitrate,
    long Max
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "subtitle-bitrate-too-high";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Subtitles Bitrate Too High"),
        (Const.CzechCulture, "Titulky mají příliš vysoký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of subtitles stream #{StreamIndex} of '{ShardName}' is too high. "
                + "It is {Bitrate:bps} but the maximum allowed subtitles bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate titulkového proudu #{StreamIndex} střípku '{ShardName}' je příliš vysoký. "
                + "Je roven {Bitrate:bps}, ale maximální povolený bitrate titulků je {Max:bps}."
        )
    );
}
