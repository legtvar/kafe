using System;

namespace Kafe.Media.Diagnostics;

public record MediaConversionFailedDiagnostic(
    Type ShardType,
    Hrib ShardId,
    string Variant
) : IDiagnosticPayload
{
    public static string Moniker => "media-conversion-failed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Conversion Failed"),
        (Const.CzechCulture, "Konverze média selhala")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{ShardType:H} '{ShardId}' could not be converted to variant '{Variant}'."),
        (Const.CzechCulture, "{ShardType:H} '{ShardId}' se nepodařilo zkonvertovat s nastavením '{Variant}'.")
    );

    public string? Reason { get; init; }
}

