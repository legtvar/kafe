namespace Kafe.Core.Diagnostics;

public record StringTooShortDiagnostic(
    LocalizedString Value,
    int MinLength
) : IDiagnosticPayload
{
    public static string Moniker => "string-too-short";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String Too Short"),
        (Const.CzechCulture, "Příliš krátký řetězec")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "String '{Value}' is too short. Minimum required length is {MinLength} characters (inclusive)."
        ),
        (
            Const.CzechCulture,
            "Řetězec '{Value}' je příliš krátký. Jeho minimální povolená délka je {MinLength} znaků (včetně)."
        )
    );
}
