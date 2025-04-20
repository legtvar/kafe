using System;

namespace Kafe.Media.Diagnostics;

public record MissingVideoStreamDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "missing-video-stream";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Video Stream"),
        (Const.CzechCulture, "Chybí video proud")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
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
