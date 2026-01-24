using System;

namespace Kafe.Media.Diagnostics;

public record MissingVideoStreamDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker => "missing-video-stream";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing a Required Video Stream"),
        (Const.CzechCulture, "Povinný video proud chybí")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Video stream #{StreamIndex} of shard '{ShardName}' is required, but it is missing."
        ),
        (
            Const.CzechCulture,
            "Video proud #{StreamIndex} střípku '{ShardName}' je povinný, ale chybí."
        )
    );
}
