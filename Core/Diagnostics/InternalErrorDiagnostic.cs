using System;

namespace Kafe.Core.Diagnostics;

public record InternalErrorDiagnostic(
    Exception? Exception
)
{
    public const string DiagnosticId = "internal-error";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Internal Error"),
        (Const.CzechCulture, "Chyba systému")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "An internal system error occurred."),
        (Const.CzechCulture, "Došlo k interní chybě systému.")
    );
}
