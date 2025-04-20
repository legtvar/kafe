using System;

namespace Kafe.Core.Diagnostics;

public record InternalErrorDiagnostic(
    Exception? Exception
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "internal-error";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Internal Error"),
        (Const.CzechCulture, "Chyba systému")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "An internal system error occurred."),
        (Const.CzechCulture, "Došlo k interní chybě systému.")
    );
}
