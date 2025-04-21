using System.Collections.Immutable;

namespace Kafe.Media.Diagnostics;

public record VideoFramerateNotAllowedDiagnostic(
    Hrib ShardId,
    LocalizedString ShardName,
    double Framerate,
    ImmutableArray<double> AllowedFramerates,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "video-framerate-not-allowed";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Video Framerate Not Allowed"),
        (Const.CzechCulture, "Nepovolená snímková frekvence videa")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Video stream #{StreamIndex} of shard '{ShardName}' has {Framerate:F3} FPS, "
                + "which is not allowed. Use one of the following framerates instead: {AllowedFramerates:F3} FPS."
        ),
        (
            Const.CzechCulture,
            "Video proud #{StreamIndex} střípku '{ShardName}' má zakázanou snímkovou frekvenci {Framerate:F3} FPS. "
                + "Použijte místo ní jednu z následujících snímkových frekvencí: {AllowedFramerates}."
        )
    );
}
