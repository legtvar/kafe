using System;

namespace Kafe.Core.Diagnostics;

public record InternalErrorDiagnostic(
    Exception? Exception
) : IDiagnosticPayload
{
    public static string Moniker => "internal-error";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Internal Error"),
        (Const.CzechCulture, "Chyba systému")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "An internal system error occurred."),
        (Const.CzechCulture, "Došlo k interní chybě systému.")
    );
}
