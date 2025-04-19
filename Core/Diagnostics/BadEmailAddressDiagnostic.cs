namespace Kafe.Core.Diagnostics;

public record BadEmailAddressDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Name { get; } = "bad-email-address";
    
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Email Address"),
        (Const.CzechCulture, "Neplatný emailová adresa")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid email address."),
        (Const.CzechCulture, "Řetězec '{Value}' není platnou emailovou adresou.")
    );
}
