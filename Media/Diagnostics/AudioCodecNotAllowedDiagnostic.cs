using System.Collections.Immutable;

namespace Kafe.Media.Diagnostics;

public record AudioCodecNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    string Codec,
    ImmutableArray<string> AllowedCodecs,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker => "audio-codec-not-allowed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Audio Codec Not Allowed"),
        (Const.CzechCulture, "Nepovolený audio kodek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Audio stream #{StreamIndex} of shard '{ShardName}' has the '{Codec}' audio codec. "
                + "This codec is not allowed. Use one of the following audio codecs instead: {AllowedCodecs}."
        ),
        (
            Const.CzechCulture,
            "Audio proud #{StreamIndex} střípku '{ShardName}' má zakázaný audio kodek '{Codec}'. "
                + "Použijte místo něj jeden z následujících audio kodeků: {AllowedCodecs}."
        )
    );
}
