using System.Collections.Immutable;

namespace Kafe.Media.Diagnostics;

public record SubtitleCodecNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    string Codec,
    ImmutableArray<string> AllowedCodecs,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker => "subtitle-codec-not-allowed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Subtitles Codec Not Allowed"),
        (Const.CzechCulture, "Nepovolený titulkový kodek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Subtitles stream #{StreamIndex} of shard '{ShardName}' has the '{Codec}' subtitles codec. "
                + "This codec is not allowed. Use one of the following subtitles codecs instead: {AllowedCodecs}."
        ),
        (
            Const.CzechCulture,
            "Titulkový proud #{StreamIndex} střípku '{ShardName}' má zakázaný titulkový kodek '{Codec}'. "
                + "Použijte místo něj jeden z následujících titulkových kodeků: {AllowedCodecs}."
        )
    );
}
