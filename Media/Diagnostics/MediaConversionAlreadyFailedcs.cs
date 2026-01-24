using System;

namespace Kafe.Media.Diagnostics;

public class MediaConversionAlreadyFailed(
    Hrib ConversionId
) : IDiagnosticPayload
{
    public static string Moniker => "media-conversion-already-failed";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Conversion Has Already Failed"),
        (Const.CzechCulture, "Konverze média již selhala")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Cannot perform conversion '{ConversionId}' because it has already failed. Please create another conversion."
        ),
        (
            Const.CzechCulture,
            "Konverzi média '{ConversionId}' nelze provést, jelikož již selhala. Prosím zahajte konverzi znovu."
        )
    );
}
