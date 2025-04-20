using System;

namespace Kafe.Media.Diagnostics;

public record SubtitleBitrateTooLowDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex,
    int Min
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "subtitle-bitrate-too-low";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Subtitles Bitrate Too Low"),
        (Const.CzechCulture, "Titulky mají příliš nízký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The bitrate of subtitles stream #{StreamIndex} of '{ShardName}' is too low. "
                + "Minimum required subtitles bitrate is {Min:bps}."
        ),
        (
            Const.CzechCulture,
            "Bitrate titulkového proudu #{StreamIndex} střípku '{ShardName}' je příliš nízký. "
                + "Minimální požadovaný bitrate titulků je {Min:bps}."
        )
    );
}
