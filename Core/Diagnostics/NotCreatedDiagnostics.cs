using System;

namespace Kafe.Core.Diagnostics;

public record NotCreatedDiagnostic(
    Type EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "not-created";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Not Created Yet"),
        (Const.CzechCulture, "Zatím nevytvořeno")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' could not be found because it has not been created yet."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' se nepodařilo nalézt, protože zatím neexistuje.")
    );
}
