using System;
using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "internal-error")]
public record InternalErrorDiagnostic(
    Exception? Exception
)
{
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
