namespace Kafe.Core.Diagnostics;

public record StringTooLongDiagnostic(
    LocalizedString Value,
    int MaxLength
) : IDiagnosticPayload
{
    public static string Moniker => "string-too-long";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String Too Long"),
        (Const.CzechCulture, "Příliš dlouhý řetězec")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "String '{Value}' is too long. Maximum allowed length is {MaxLength} characters (inclusive)."
        ),
        (
            Const.CzechCulture,
            "Řetězec '{Value}' je příliš dlouhý. Jeho maximální povolená délka je {MaxLength} znaků (včetně)."
        )
    );
}
