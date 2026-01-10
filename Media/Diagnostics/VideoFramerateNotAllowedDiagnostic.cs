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
    public static string Moniker => "video-framerate-not-allowed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Video Framerate Not Allowed"),
        (Const.CzechCulture, "Nepovolená snímková frekvence videa")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
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
