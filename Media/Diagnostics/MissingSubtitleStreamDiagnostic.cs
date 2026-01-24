using System;

namespace Kafe.Media.Diagnostics;

public record MissingSubtitleStreamDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker => "missing-subtitle-stream";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing a Required Subtitle Stream"),
        (Const.CzechCulture, "Povinný titulkový proud chybí")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
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
