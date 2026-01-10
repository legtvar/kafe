using System;

namespace Kafe.Media.Diagnostics;

public record MediaTooLongDiagnostic(
    LocalizedString ShardName,
    Hrib ShardId,
    string Variant,
    TimeSpan MaxDuration
) : IDiagnosticPayload
{
    public static string Moniker => "media-too-long";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Too Long"),
        (Const.CzechCulture, "Příliš dluhé médium")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Shard '{ShardName}' is too long. Maximum allowed duration is '{MaxDuration:c}'."),
        (Const.CzechCulture, "Střípek '{ShardName}' je příliš dlouhý. Maximální povolená délka je '{MaxDuration:c}'.")
    );
}
