using System;

namespace Kafe.Media.Diagnostics;

public record MediaBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int Max
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "media-bitrate-too-high";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Media Bitrate Too High"),
        (Const.CzechCulture, "Médium má příliš vysoký bitrate")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The total bitrate of shard '{ShardName}' is too high. Maximum allowed bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Celkový itrate střípku '{ShardName}' je příliš vysoký. Maximální povolený bitrate je {Max:bps}."
        )
    );
}
