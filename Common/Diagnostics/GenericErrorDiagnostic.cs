using System.Globalization;

namespace Kafe;

public class GenericErrorDiagnostic : IDiagnosticPayload
{
    public static string? Moniker => "generic-error";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString? Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Something Bad Happened"),
        (Const.CzechCulture, "Něco se pokazilo"),
        (Const.SlovakCulture, "Niečo sa pokazilo")
    );

    public static LocalizedString? Description { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Oh no... An unspecified error occurred and unfortunately there is no additional information available."
        ),
        (
            Const.CzechCulture,
            "Ajéje... Došlo k blíže neurčené chybě a žádné další detaily bohužel nejsou k dispozici."
        ),
        (
            Const.SlovakCulture,
            "Ajéje... Došlo k bližšie neurčenej chybe a žiadne ďalšie detaily bohužiaľ nie sú k dispozícii."
        )
    );

    public static LocalizedString? MessageFormat => Description;
}
