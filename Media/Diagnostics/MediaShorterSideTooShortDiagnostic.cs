using System;

namespace Kafe.Media.Diagnostics;

public record MediaShorterSideTooShortDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int Min
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "media-shorter-side-too-short";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Media Shorter Side Too Short"),
        (Const.CzechCulture, "Kratší strana média je příliš krátká")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The shorter side of media shard '{ShardName}' is too short. It must be at least {Min} pixels long."
        ),
        (
            Const.CzechCulture,
            "Kratší strana mediového střípku '{ShardName}' je příliš krátká. Musí mít alespoň {Min} pixelů."
        )
    );
}
