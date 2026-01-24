using System;

namespace Kafe.Media.Diagnostics;

public class MediaConversionAlreadyCompleted(
    Hrib ConversionId
) : IDiagnosticPayload
{
    public static string Moniker => "media-conversion-already-completed";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Media Conversion Is Already Completed"),
        (Const.CzechCulture, "Konverze média již úspěšně proběhla")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Cannot perform conversion '{ConversionId}' because it has already successfully completed."
        ),
        (
            Const.CzechCulture,
            "Konverzi média '{ConversionId}' nelze provést, jelikož již úspěšně proběhla."
        )
    );
}
