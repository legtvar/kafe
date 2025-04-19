namespace Kafe.Core.Diagnostics;

public record StringTooLongDiagnostic(
    LocalizedString Value,
    int MaxLength
) : IDiagnosticPayload
{
    public static string Name { get; } = "string-too-long";
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "String Too Long"),
        (Const.CzechCulture, "Příliš dlouhý řetězec")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is too long. Maximum allowed length is {MaxLength} characters."),
        (Const.CzechCulture, "Řetězec '{Value}' je příliš dlouhý. Jeho maximální povolená délka je {MaxLength} znaků.")
    );
}
