using System;

namespace Kafe.Media.Diagnostics;

public record MissingSubtitleStreamDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "missing-subtitle-stream";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Subtitle Stream"),
        (Const.CzechCulture, "Chybí titulkový proud")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Subtitles stream #{StreamIndex} of shard '{ShardName}' is required, but it is missing."
        ),
        (
            Const.CzechCulture,
            "Proud titulků #{StreamIndex} střípku '{ShardName}' je povinný, ale chybí."
        )
    );
}
