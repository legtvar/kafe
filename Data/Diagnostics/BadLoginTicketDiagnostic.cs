namespace Kafe.Data.Diagnostics;

public record BadLoginTicketDiagnostic : IDiagnosticPayload
{
    public static string Moniker => "bad-login-ticket";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Login Ticket"),
        (Const.CzechCulture, "Neplatná žádost o přihlášení")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "The login ticket is invalid, or has expired."),
        (Const.CzechCulture, "Žádost o přihlášení je neplatná nebo již expirovala.")
    );
}
