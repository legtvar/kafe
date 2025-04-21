using System.Collections.Immutable;

namespace Kafe.Media.Diagnostics;

public record VideoCodecNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    string Codec,
    ImmutableArray<string> AllowedCodecs,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "video-codec-not-allowed";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Video Codec Not Allowed"),
        (Const.CzechCulture, "Nepovolený video kodek")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Video stream #{StreamIndex} of shard '{ShardName}' has the '{Codec}' video codec. "
                + "This codec is not allowed. Use one of the following video codecs instead: {AllowedCodecs}."
        ),
        (
            Const.CzechCulture,
            "Video proud #{StreamIndex} střípku '{ShardName}' má zakázaný video kodek '{Codec}'. "
                + "Použijte místo něj jeden z následujících video kodeků: {AllowedCodecs}."
        )
    );
}
