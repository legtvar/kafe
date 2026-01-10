namespace Kafe.Core.Diagnostics;

public record BadEmailAddressDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Moniker => "bad-email-address";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Email Address"),
        (Const.CzechCulture, "Neplatný emailová adresa")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid email address."),
        (Const.CzechCulture, "Řetězec '{Value}' není platnou emailovou adresou.")
    );
}
