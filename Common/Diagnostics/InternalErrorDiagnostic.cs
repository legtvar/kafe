using System;

namespace Kafe;

public record InternalErrorDiagnostic(
    Type ExceptionType,
    string ExceptionMessage
) : IDiagnosticPayload
{
    public static string Moniker => "internal-error";

    public static DiagnosticSeverity DefaultSeverity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Internal Error"),
        (Const.CzechCulture, "Interní chyba")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Something not-all-that-great happened... and it's our fault. [{ExceptionType}]"),
        (Const.CzechCulture, "„Máme tady jeden takový ošklivý, nepěkná věc“ ...a je to naše chyba. [{ExceptionType}]")
    );
}
