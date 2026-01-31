using System;

namespace Kafe.Data.Diagnostics;

public record EntityBusyDiagnostic(
    Type EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "entity-busy";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Entity Busy"),
        (Const.CzechCulture, "Entita zaneprázdněn")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture,
            "Operation on {EntityType:h} '{Id}' could not be completed, because the entity is busy. Please try again."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' je aktuálně používán, tudíž operace nemohla být dokončena.")
    );
}
