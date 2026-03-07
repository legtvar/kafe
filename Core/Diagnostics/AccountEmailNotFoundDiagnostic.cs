using System;

namespace Kafe.Core.Diagnostics;

public record AccountEmailNotFoundDiagnostic(
    string EmailAddress
) : IDiagnosticPayload
{
    public static string Moniker => "account-email-not-found";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Account Not Found"),
        (Const.CzechCulture, "Účet Nenalezen")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Account '{EmailAddress}' could not be found. Are you sure it has been registered?"),
        (Const.CzechCulture, "Účet '{EmailAddress}' se nepodařilo nalézt. Jste si jistí, že byl zaregistrován?")
    );
}
