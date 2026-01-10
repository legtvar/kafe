using System;

namespace Kafe.Media.Diagnostics;

public record MissingAudioStreamDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int StreamIndex
) : IDiagnosticPayload
{
    public static string Moniker => "missing-audio-stream";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Audio Stream"),
        (Const.CzechCulture, "Chybí audio proud")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Audio stream #{StreamIndex} of shard '{ShardName}' is required, but it is missing."
        ),
        (
            Const.CzechCulture,
            "Audio proud #{StreamIndex} střípku '{ShardName}' je povinný, ale chybí."
        )
    );
}
