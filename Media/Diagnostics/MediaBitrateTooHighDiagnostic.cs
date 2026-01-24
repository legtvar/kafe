using System;

namespace Kafe.Media.Diagnostics;

public record MediaBitrateTooHighDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string? Variant,
    int Max
) : IDiagnosticPayload
{
    public static string Moniker => "media-bitrate-too-high";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Bitrate Too High"),
        (Const.CzechCulture, "Médium má příliš vysoký bitrate")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "The total bitrate of shard '{ShardName}' is too high. Maximum allowed bitrate is {Max:bps}."
        ),
        (
            Const.CzechCulture,
            "Celkový bitrate střípku '{ShardName}' je příliš vysoký. Maximální povolený bitrate je {Max:bps}."
        )
    );
}
