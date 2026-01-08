namespace Kafe.Core.Diagnostics;

public record MissingAuthorNameOrIdDiagnostic : IDiagnosticPayload
{
    public static string Moniker => "missing-author-name-or-id";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Author Name or Reference"),
        (Const.CzechCulture, "Chybí jméno nebo odkaz na autora")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Either a reference to an existing author or at least their name is required."),
        (Const.CzechCulture, "Je nutné nastavit odkaz na existujícího autora nebo přinejmenším jeho jméno.")
    );
}
