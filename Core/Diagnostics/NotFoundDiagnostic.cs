using System;

namespace Kafe.Core.Diagnostics;

public record NotFoundDiagnostic(
    Type EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "not-found";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Not Found"),
        (Const.CzechCulture, "Nenalezeno")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' could not be found. Are you sure it exists?"),
        (Const.CzechCulture, "{EntityType:h} '{Id}' se nepodařilo nalézt. Jste si jistí, že existuje?")
    );
}
